﻿
//===============================================================================
//
//  IMPORTANT NOTICE, PLEASE READ CAREFULLY:
//
//  ● This code is dual-licensed (GPLv3 + Commercial). Commercial licenses can be obtained from: http://cshtml5.com
//
//  ● You are NOT allowed to:
//       – Use this code in a proprietary or closed-source project (unless you have obtained a commercial license)
//       – Mix this code with non-GPL-licensed code (such as MIT-licensed code), or distribute it under a different license
//       – Remove or modify this notice
//
//  ● Copyright 2019 Userware/CSHTML5. This code is part of the CSHTML5 product.
//
//===============================================================================


using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

#if BRIDGE
using Bridge;
#endif

namespace System
{
    internal class INTERNAL_WebRequestHelper_JSOnly
    {
        dynamic _xmlHttpRequest;

        /// <summary>
        /// Occurs when the string download is completed.
        /// </summary>
        public event INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventHandler DownloadStringCompleted;

        /// <summary>
        /// Initializes a new instance of the INTERNAL_WebRequestHelper class.
        /// </summary>
        public INTERNAL_WebRequestHelper_JSOnly() { }

        static Dictionary<string, bool> WebServiceUrlToJsCredentialsSupported = new Dictionary<string,bool>();

        // copy the parameters for resending method in case of fail
        Uri _address;
        string _Method;
        Dictionary<string, string> _headers;
        string _body;
        bool _isAsync;
        INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventHandler _callback;
        static INTERNAL_WebRequestHelper_JSOnly _requester;

        /// <summary>
        /// Makes a synchronous or asynchronous request at the specified uri, using the specified method, with the specified headers and body, and calls the callbackMethod.
        /// </summary>
        /// <param name="address">the uri that identifies the Internet resource.</param>
        /// <param name="Method">The method to be called after making the request.</param>
        /// <param name="headers">
        /// A dictionary containing the headers to put in the method.
        /// The pairs key/values in the dictionary correspond to the pairs key/value in the headers.
        /// </param>
        /// <param name="body">The body of the request.</param>
        /// <param name="callbackMethod">The method to be called after the request has been made.</param>
        /// <param name="isAsync">A boolean that determines whether the request must be made synchronously or asynchronously.</param>
        /// <returns>The result of the request as a string.</returns>
        public string MakeRequest(Uri address, string Method, Dictionary<string, string> headers, string body, INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventHandler callbackMethod, bool isAsync, CredentialsMode mode = CredentialsMode.Disabled)
        {
            bool askForUnsafeRequest = false; // This is true if we are doing the initial request to determine whether the credentials are supported or not.

            _xmlHttpRequest = GetWebRequest();

            //define the action to do when the xmlhttp has finished the request:
            if (callbackMethod != null)
            {
                DownloadStringCompleted -= callbackMethod;
                DownloadStringCompleted += callbackMethod;
            }
            SetCallbackMethod((object)_xmlHttpRequest, OnDownloadStringCompleted);

            //create the request:
            CreateRequest((object)_xmlHttpRequest, address.OriginalString, Method, isAsync);

            if(!WebServiceUrlToJsCredentialsSupported.ContainsKey(address.OriginalString))
            {
                if(mode == CredentialsMode.Auto)
                {
                    WebServiceUrlToJsCredentialsSupported.Add(address.OriginalString, true); // if not supported, this value will be changed soon
                    askForUnsafeRequest = true;
                }
                else
                {
                    WebServiceUrlToJsCredentialsSupported.Add(address.OriginalString, mode == CredentialsMode.Disabled ? false : true);
                }
            }

            // XHR does not allow to read the 'Set-Cookie' header and to write the 
            // 'cookie' header. The command withCredentials = true must be use to
            // automatically handle cookies. But from server side Access-control-orginal must not be '*'
            // so some modifications must be done from server side too.
            //https://developer.mozilla.org/fr/docs/Web/HTTP/CORS
            // With credentials, the 'Origin' header must not be null, but XHR does not allow direct modifications
            // on "Origin" and running the app from the file always set the Origin to null:
            // to test credential, make sure to run it in local server
            // headers.Add("Origin", "http://Something.com/");
            EnableCookies((object)_xmlHttpRequest, WebServiceUrlToJsCredentialsSupported[address.OriginalString]);

            if (headers != null && headers.Count > 0)
            {
                foreach (string key in headers.Keys)
                {
                    SetRequestHeader((object)_xmlHttpRequest, key, headers[key]);
                }
            }

            if (askForUnsafeRequest) // if the settings of the request are still unsafe
            {
                // handle special errors especially crash in pre flight, that GetHasError doesn't catch
                SetErrorCallback((object)_xmlHttpRequest, OnError);

                // save the inputs to resend the request in case of error
                SaveParameters(address, Method, headers, callbackMethod, body, isAsync);

                // safe request, will resend the request with different settings if it crashes.
                return SendUnsafeRequest((object)_xmlHttpRequest, address.OriginalString, Method, isAsync, body);
            }
            else
            {
                SendRequest((object)_xmlHttpRequest, address.OriginalString, Method, isAsync, body);
            }
            
            if (GetHasError((object)_xmlHttpRequest))
            {
                throw new Exception("The remote server has returned an error: (" + GetCurrentStatus((object)_xmlHttpRequest) + ") " + GetCurrentStatusText((object)_xmlHttpRequest) + ".");
            }

            //get the response:
            return GetResult((object)_xmlHttpRequest);
        }

        // special version of sendRequest, it handles some errors and modifies the credentials mode if needed
        // return directly the result of the right response
        private string SendUnsafeRequest(object xmlHttpRequest, string address, string method, bool isAsync, string body)
        {
            ConsoleLog_JSOnly("CredentialsMode is set to Auto: if a preflight error appears below, please ignore it.");

            if (!isAsync) // if synchronous mode, we must handle any problem in the request just after it happens
            {
                try
                {
                    SendRequest((object)_xmlHttpRequest, address, method, isAsync, body);

                    ResendRequestInCaseOfPreflightError(false);
                    return GetResult((object)_xmlHttpRequest);
                }
                catch
                {
                    // try new credentials settings and resend this request
                    if (IsCrashInPreflight)
                    {
                        return ResendRequestInCaseOfPreflightError(true);
                    }
                    else
                    {
                        ResendRequestInCaseOfPreflightError(false);
                        return GetResult((object)_xmlHttpRequest); // normally, in this method, crash are due to preflight errors, we are not suppose to arrive here
                    }
                }
            }
            else
            {
                // in asynchronous mode, the error callback will directly arrive in errorOnSetting, and it will resend this request
                SendRequest((object)_xmlHttpRequest, address, method, isAsync, body);
                return GetResult((object)_xmlHttpRequest);
            }
        }

        // if the last request was asynchronous, any error will be cought here.
        private void OnError(object sender)
        {
            if (IsCrashInPreflight)
                ResendRequestInCaseOfPreflightError(true);
        }

        // if the last request has crashed, we modify the settings and we resend the last request
        // the return result is only useful in synchronous mode
        private string ResendRequestInCaseOfPreflightError(bool error)
        {
            string newResult = string.Empty;

            if (error)
            {
                WebServiceUrlToJsCredentialsSupported[_address.OriginalString] = false; // not supported

                // resend the request with the new setting
                newResult = ResendLastUnsafeRequest();

                ConsoleLog_JSOnly("The requested server does not seem to accept credentials. To stop getting the error above, make sure to set CredentialsMode to Disabled. To do so with REST calls, please set the property WebClientWithCredentials.CredentialsMode to Disabled. For SOAP calls, please place the following code in your application constructor: Application.Current.Host.Settings.DefaultSoapCredentialsMode = System.Net.CredentialsMode.Disabled;");
            }

            ConsoleLog_JSOnly("Credentials status is now confirmed: no other preflight errors are expected for this request.");

            return newResult;
        }

        private bool IsCrashInPreflight
        {
            get
            {
                //CurrentReadyState = 4 and CurrentStatus = 0 (unsend) seem to mean error in preflight, but no more informations
                return (GetCurrentReadyState((object)_xmlHttpRequest) == 4 && GetCurrentStatus((object)_xmlHttpRequest) == 0);
            }
        }

        private static string ResendLastUnsafeRequest()
        {
            // we need to recreate a webRequestHelper, beacause we can't modify settings after the request was send
            return new INTERNAL_WebRequestHelper_JSOnly().MakeRequest(_requester._address, _requester._Method, _requester._headers, _requester._body, _requester._callback, _requester._isAsync, CredentialsMode.Disabled);
        }

        private void SaveParameters(Uri address, string Method, Dictionary<string, string> headers, INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventHandler callback, string body, bool isAsync)
        {
            _address = address;
            _Method = Method;
            _headers = headers;
            _body = body;
            _isAsync = isAsync;
            _callback = callback;
            _requester = this;
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.setRequestHeader($key,$header)")]
#else
        [Template("{xmlHttpRequest}.setRequestHeader({key},{header})")]
#endif
        private static void SetRequestHeader(object xmlHttpRequest, string key, string header)
        {
            //do nothing
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("new XMLHttpRequest()")]
#else
        [Template("new XMLHttpRequest()")]
#endif
        internal static dynamic GetWebRequest()
        {
            throw new NotImplementedException(); //find a better exception for here (we are not supposed to arrive here in C#).
        }
         
#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.onload = $OnDownloadStatusCompleted")]
#else
        [Template("{xmlHttpRequest}.onload = {OnDownloadStatusCompleted}")]
#endif

        internal static void SetCallbackMethod(object xmlHttpRequest, Action<object, INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs> OnDownloadStatusCompleted)
        {
            //do nothing
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.open($method, $address, $isAsync)")]
#else
        [Template("{xmlHttpRequest}.open({method}, {address}, {isAsync})")]
#endif
        private static void CreateRequest(object xmlHttpRequest, string address, string method, bool isAsync)
        {
            //do nothing
        }


#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.withCredentials = $value")]
#else
        [Template("{xmlHttpRequest}.withCredentials = {value}")]
#endif
        private static void EnableCookies(object xmlHttpRequest, bool value)
        {
            throw new NotImplementedException();
        }


#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.onerror = $OnError")]
#else
        [Template("{xmlHttpRequest}.onerror = {OnError}")]
#endif
        internal static void SetErrorCallback(object xmlHttpRequest, Action<object> OnError)
        {
            //do nothing
        }


#if !BRIDGE
        [JSIL.Meta.JSReplacement("console.log($message);")]
#else
        [Template("console.log({message});")]
#endif
        internal static void ConsoleLog_JSOnly(string message)
        {
            //do nothing
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.send($body)")]
#else
        [Template("{xmlHttpRequest}.send({body})")]
#endif
        internal static void SendRequest(object xmlHttpRequest, string address, string method, bool isAsync, string body)
        {
            //do nothing
        }

        private void OnDownloadStringCompleted(object sender, INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs e)
        {
            e = new INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs();
            SetEventArgs(e);
            if (DownloadStringCompleted != null)
            {
                DownloadStringCompleted(sender, e);
            }
        }

        private void SetEventArgs(INTERNAL_WebRequestHelper_JSOnly_RequestCompletedEventArgs e)
        {
            int currentReadyState = GetCurrentReadyState((object)_xmlHttpRequest);
            int currentStatus = GetCurrentStatus((object)_xmlHttpRequest);
            if (currentStatus == 404)
            {
                e.Error = new Exception("Page not found");
            }
            else if (currentReadyState == 0 && !e.Cancelled)
            {
                e.Error = new Exception("Request not initialized");
            }
            else if (currentReadyState == 1 && !e.Cancelled)
            {
                e.Error = new Exception("An Error occured. Cross-Site Http Request might not be allowed at the target Url. If you own the domain of the Url, consider adding the header \"Access-Control-Allow-Origin\" to enable requests to be done at this Url.");
            }
            else if (currentReadyState != 4)
            {
                e.Error = new Exception("An Error has occured while submitting your request.");
            }
            e.Result = GetResult((object)_xmlHttpRequest);
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.readyState")]

#else
        [Template("{xmlHttpRequest}.readyState")]
#endif
        private static int GetCurrentReadyState(object xmlHttpRequest)
        {
            throw new NotImplementedException();
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.status")]

#else
        [Template("{xmlHttpRequest}.status")]
#endif

        private static int GetCurrentStatus(object xmlHttpRequest)
        {
            throw new NotImplementedException();
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.statusText")]

#else
        [Template("{xmlHttpRequest}.statusText")]
#endif
        private static int GetCurrentStatusText(object xmlHttpRequest)
        {
            throw new NotImplementedException();
        }

#if !BRIDGE
        [JSIL.Meta.JSReplacement("$xmlHttpRequest.responseText")]

#else
        [Template("{xmlHttpRequest}.responseText")]
#endif

        private static string GetResult(object xmlHttpRequest)
        {
            throw new NotImplementedException();
        }

        private static bool GetHasError(object xmlHttpRequest)
        {
            int currentStatus = GetCurrentStatus(xmlHttpRequest);
            //note: 4XX status corresponds to a client error (basically the  request is badly formatted or unauthorized... so it is not accepted by the server)
            //      5XX status corresponds to a server error (it happened to us with ClientBase when the serialization of the objects passed as parameters was not properly done and couldn't be deserialized)
            if (currentStatus >= 400 && currentStatus < 600)
            {
                return true;
            }
            return false;
        }


        //todo: see if this should be removed or not (I think it should)
        internal object GetXmlHttpRequest()
        {
            return _xmlHttpRequest;
        }
    }
}
