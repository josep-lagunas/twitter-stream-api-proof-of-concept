/**
 * Created by Josep on 18/12/14.
 */
/**
 *  @author: Josep Lagunas
 *  @date: 02/04/2014
 *  @dependences: jQuery v1.11.0
 */

(function(){

    var serverConnector = (function(){

        var instance = "";
        var _baseURL = "";
        var _baseWebsocketUrl = "";

        function init(){

            //Singleton

            //Private Methods and Variables goes here

            //check method to assure that base URL has been set before any call.
            function isBaseURLSet(){

                if (_baseURL === "" || _baseURL === undefined)
                    throw new Error("Base URL must be set before. Use ServerConnector.setBaseURL() method.");
            }

            //Extract and adapt parameters (queryParameters)
            function extractQueryParameters(parameters){

                var queryParametersURLFormat = "";

                if (parameters !== undefined && parameters !== ""){

                    parameters.forEach(function(current){

                        if (typeof(current.key) === "string" || (typeof(current.value) === "number" || typeof(current.value) === "string"))
                        {

                            if (current.key !== undefined && current.key !== ""){
                                queryParametersURLFormat = (queryParametersURLFormat === "")?  "?" + current.key + "=" + current.value : queryParametersURLFormat + "&" + current.key + "=" + current.value;
                            }

                        }else

                            throw new Error("Invalid query parameter type, only string and number types allowed");

                    });

                }

                return queryParametersURLFormat;
            }

            //Extract and adapt parameters (pathParameters)
            function extractPathParameters(parameters){

                var PathParameters = "";

                if (parameters !== undefined && parameters !== ""){

                    parameters.forEach(function(current){

                        if (typeof(current.key) === "string" || (typeof(current.value) === "number" || typeof(current.value) === "string"))
                        {

                            if (current.key !== undefined && current.key !== ""){
                                PathParameters +=  "/" + current.value;
                            }

                        }else

                            throw new Error("Invalid path paramater type, only string and number types allowed");

                    });

                }

                return PathParameters;
            }

            return {
                /*
                 *	This method wrap a call to server api REST (GET,POST,PUT) passing parameters as
                 * 	query string for
                 *
                 */
                callRESTServerMethod: function () {


                    //check for base URL
                    isBaseURLSet();

                    if (arguments.length === 0)
                        return;

                    var settings = arguments[0];

                    var checkVerb = settings["verb"];

                    var method = settings["method"];

                    if (method === undefined)
                        throw new Error("A method must specified");

                    method = "/" + method;

                    if (checkVerb === undefined)
                        throw new Error("A VERB must be specified");

                    if (checkVerb === "GET") {
                        // Body parameters are not allowed
                        // Only query and/or path parameters allowed
                        var bodyParameters = settings["bodyParameters"];
                        if (bodyParameters !== undefined) {
                            throw new Error("Invalid use of bodyParameters in a GET call");
                        }
                    }
                    //					}else if (checkVerb === "POST" || checkVerb === "PUT"){
                    //						//No checs needed
                    //					}

                    var async = settings["async"] || true;
                    var verb = checkVerb;
                    var getAsJSON = settings["getAsJSON"] || true;

                    var queryParameters = settings["queryParameters"];
                    var queryParametersURLFormat = extractQueryParameters(queryParameters);

                    var pathParameters = settings["pathParameters"];
                    var pathParametersURL = extractPathParameters(pathParameters);

                    var bodyParameters = settings["bodyParameters"] || "";
                    bodyParameters = (bodyParameters !== "" && bodyParameters !== undefined) ? bodyParameters : "";
                    //we have to check if we have received the body parameter already stringified or not
                    if (typeof (bodyParameters) !== "string")
                        bodyParameters = JSON.stringify(bodyParameters);


                    var callback = settings["callback"];
                    var errorCallback = settings["errorCallback"];
                    var beforeSendCallback = settings["beforeSendCallback"];
                    var completedCallback = settings["completedCallback"];

                    jQuery.ajax({
                        type: verb,
                        url: _baseURL + method + pathParametersURL + queryParametersURLFormat,
                        data: bodyParameters,
                        dataType: "text",
                        async: async,
                        contentType: "application/json; charset=utf-8",
                        success: function (data, textStatus) {

                            //Check Session Time Out
                            //CheckAjaxSessionValidation(data.d);

                            var result = "";
                            if (getAsJSON == true) {
                                try {
                                    result = JSON.parse(data);
                                } catch (e) { }
                            } else {
                                result = data;
                            }

                            if (callback !== undefined && callback !== null)
                                callback(textStatus, result);
                        },
                        error: function (msg) {

                            if (errorCallback !== undefined && errorCallback !== null)
                                errorCallback(msg.responseText);

                        },
                        beforeSend: beforeSendCallback,
                        complete: completedCallback
                    });
                },
                callWebSocketMethod: function () {

                    //check for base URL
                    isBaseURLSet();

                    if (arguments.length === 0)
                        return;

                    var settings = arguments[0];

                    var method = settings["method"];

                    if (method === undefined)
                        throw new Error("A method must specified");

                    method = "/" + method;
                    
                    var onOpenCallback = settings["onOpenCallback"];
                    var onMessageCallback = settings["onMessageCallback"];
                    var onErrorCallback = settings["onErrorCallback"];
                    var onCloseCallback = settings["onCloseCallback"];
                    
                    var url = _baseWebsocketUrl + method;

                    var connection = new WebSocket(url);
                                        
                    return connection;
                }
            }
        };

        return{

            setBaseURL: function(baseURL){

                if (baseURL.indexOf('https') == -1 && baseURL.indexOf('http') == -1) {
                    throw Error("http or https not found.")
                }
                _baseURL = baseURL;
                _baseWebsocketUrl = baseURL.replace('https', 'ws').replace('http', 'ws');

            },

            getBaseURL: function(){

                return _baseURL;
            },
            getBaseWebSocketURL: function () {

                return _baseWebsocketUrl;

            },
            getInstance: function(){

                if (!instance){
                    instance = init();
                }

                return instance;
            }
        };

    })();

    window["serverConnector"] = serverConnector;

})();

