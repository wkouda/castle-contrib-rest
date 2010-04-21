// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// Music Glue: this Castle.MonoRail.Rest code has been modified to handle  
// preflighted requests in cross-domain contexts. A preflighted request 
// first sends an HTTP OPTIONS request header to the resource on the other  
// domain to check if the initial request is safe. 
// Only then is the initial request handled.

using System.Linq;

namespace Castle.MonoRail.Rest
{
	using System;
	using Framework;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;
	using Components.Binder;
	using Framework.Services;
	using Mime;

	public class RestfulController : SmartDispatcherController 
	{
		private string _controllerAction;
		private const string OptionsString = "Options";
		IDictionary allowedActions;
		IRequest currentRequest;

	    protected override MethodInfo SelectMethod(string action, IDictionary actions, IRequest request,
	                                               IDictionary<string, object> actionArgs, ActionType actionType)
	    {
	    	string restAction = ActionSelector.GetRestActionName(action, actions, HttpMethod);
			allowedActions = actions;
	    	currentRequest = request;
	    	
			// For Options Http methods, ignore the current   
			// action and invoke the Options() action instead
			if (HttpMethod.ToUpper() == "OPTIONS")
			{
				var optionsMethodInfo = GetType().GetMethod(OptionsString, BindingFlags.NonPublic | BindingFlags.Instance);
				if (!actions.Contains(OptionsString))
				{
					allowedActions.Add(OptionsString, optionsMethodInfo);
				}
				return optionsMethodInfo;
			}

	    	if (ActionSelector.IsCollectionAction(action))
			{
				switch (HttpMethod.ToUpper())
				{
					case "GET":
					case "POST":
					_controllerAction = restAction;
						return (MethodInfo)actions[restAction];
					default:
						return base.SelectMethod(action, actions, request, actionArgs, actionType);
				}
			}

			if (ActionSelector.IsNewAction(action))
			{
				return (MethodInfo)actions[restAction];
			}

			if (!actions.Contains(action))
			{
				switch (HttpMethod.ToUpper())
				{
					case "GET":
					case "PUT":
					case "DELETE":
						_controllerAction = restAction;
						Request.ParamsNode.AddChildNode(new LeafNode(typeof(String), "ID", action));
						return (MethodInfo)actions[restAction];
					default:
						// Should maybe just throw here.
						return base.SelectMethod(action, actions, request, actionArgs, actionType);
				}
			}
			return base.SelectMethod(action, actions, request, actionArgs, actionType);
		}

		// The response when an Options method is detected
		protected void Options()
		{
			CancelView();
			
			var allowedMethods = MethodSelector.GetAllowedMethods(allowedActions);
			if (!string.IsNullOrEmpty(allowedMethods))
			{
				Response.AppendHeader("Access-Control-Allow-Methods", allowedMethods);
			}

			var allowSettings = WebConfigFile.GetAllowConfigSettings(currentRequest);
			if (!string.IsNullOrEmpty(allowSettings["Origin"]))
			{
				Response.AppendHeader("Access-Control-Allow-Origin", allowSettings["Origin"]);
			}
			if (!string.IsNullOrEmpty(allowSettings["Header"]))
			{
				Response.AppendHeader("Access-Control-Allow-Headers", allowSettings["Header"]);
			}
			if (!string.IsNullOrEmpty(allowSettings["Credentials"]))
			{
				Response.AppendHeader("Access-Control-Allow-Credentials", allowSettings["Credentials"]);
			}
		}

		protected void RespondTo(Action<ResponseFormat> collectFormats)
		{
			MimeTypes registeredMimes = GetRegisteredMimeTypes();
			
			ResponseHandler handler = new ResponseHandler()
			{
				ControllerBridge = GetControllerBridgeForAction(_controllerAction),
				AcceptedMimes = GetAcceptedTypes(registeredMimes),
				Format = new ResponseFormat(registeredMimes)
			};

			collectFormats(handler.Format);
			handler.Respond();
		}

		protected string UrlFor(IDictionary parameters)
		{
			return Context.Services.UrlBuilder.BuildUrl(Context.UrlInfo, parameters);
		}

		protected string UrlFor(string action)
		{
			return Context.Services.UrlBuilder.BuildUrl(Context.UrlInfo, new UrlBuilderParameters(Name, action));
		}
		
		protected virtual string AcceptHeader
		{
			get { return Request.Headers["Accept"]; }
		}

		protected virtual IControllerBridge GetControllerBridgeForAction(string action)
		{
			return new ControllerBridge(this, action);
		}

		protected virtual MimeTypes GetRegisteredMimeTypes()
		{
			var mimeTypes = new MimeTypes();
			mimeTypes.RegisterBuiltinTypes();

			return mimeTypes;
		}

		protected virtual string HttpMethod
		{
			get { return Request.HttpMethod; }
		}

		private MimeType[] GetAcceptedTypes(MimeTypes registeredMimes)
		{
			var mimeTypes = new List<MimeType>();
			var originalUrl = Request.Uri.GetLeftPart(UriPartial.Authority) + Request.Url;
			var lastSegment = new Uri(originalUrl).Segments.Last();
			
			if (lastSegment.Contains(".") && (lastSegment.LastIndexOf(".") < lastSegment.Length - 1))
			{
				var extension = lastSegment.Substring(lastSegment.LastIndexOf(".") + 1);
				var mimeType = registeredMimes.GetMimeTypeForExtension(extension);

				if (mimeType != null)
					mimeTypes.Add(mimeType);
			}

			mimeTypes.AddRange(AcceptType.Parse(AcceptHeader, registeredMimes));			
			
			return mimeTypes.Distinct().ToArray();
		}
	}
}
