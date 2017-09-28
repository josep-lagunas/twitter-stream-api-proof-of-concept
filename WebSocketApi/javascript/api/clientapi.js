/**
 * Created by Josep on 18/12/14.
 */
/**
 * 	@author: Josep Lagunas
 *  @date: 02/04/2014
 */

(function () {

    var clientApi = (function () {

        var instance = "";

        function init() {

            //Initialize baseURL for serverConnector
            serverConnector.setBaseURL(document.location.origin);
            
            //Private Methods and Variables goes here

            var connection;

            return {

                requestWSToken: function (isAsync, getAsJson, successCallback, errorCallback,
                    beforeSendCallback, completedCallback) {
                    var settings = {
                        verb: "GET",
                        async: isAsync,
                        getAsJon: getAsJson,
                        method: "api/request-ws-token",
                        pathParameters: [],
                        callback: successCallback,
                        errorCallback: errorCallback,
                        beforeSendCallback: beforeSendCallback,
                        completedCallback: completedCallback
                    };
                    
                    serverConnector.getInstance().callRESTServerMethod(settings);
                },
                openWebSocketConnection: function (clientId, wsToken) {
                   
                    var settings = {
                        verb: "GET",
                        headers: { 'cliend-id': clientId },
                        method: "api/connect-websocket"                      
                    };
                    
                    return serverConnector.getInstance().callWebSocketMethod(settings);
                },
                subscribeTwitterStream: function (isAsync, getAsJson, clientId, successCallback, errorCallback,
                    beforeSendCallback, completedCallback) {

                    var settings = {
                        verb: "POST",
                        async: isAsync,
                        getAsJon: getAsJson,
                        method: "api/subscribe-server-events",
                        headers: { 'client-id' : clientId },
                        pathParameters: [],
                        bodyParameters: [{ Id: 1, Name: "GET_TWEETS" }],
                        callback: successCallback,
                        errorCallback: errorCallback,
                        beforeSendCallback: beforeSendCallback,
                        completedCallback: completedCallback
                    }

                    return serverConnector.getInstance().callRESTServerMethod(settings);
                },
                startTwitterStream: function (isAsync, getAsJson, keywords, languages, mapboxcoordinates,
                    clientId, successCallback, errorCallback, beforeSendCallback, completedCallback) {

                    var bodyParameters = JSON.stringify({ keywords: keywords, languages: languages, mapboxcoordinates: mapboxcoordinates });

                    var settings = {
                        verb: "POST",
                        async: isAsync,
                        getAsJon: getAsJson,
                        method: "api/start-streaming-tweets",
                        headers: { 'client-id': clientId },
                        pathParameters: [],
                        bodyParameters: bodyParameters,
                        callback: successCallback,
                        errorCallback: errorCallback,
                        beforeSendCallback: beforeSendCallback,
                        completedCallback: completedCallback
                    }

                    return serverConnector.getInstance().callRESTServerMethod(settings);
                },
                stopTwitterStream: function (isAsync, getAsJson, clientId, successCallback, errorCallback,
                    beforeSendCallback, completedCallback) {

                    var settings = {
                        verb: "POST",
                        async: isAsync,
                        getAsJon: getAsJson,
                        method: "api/stop-streaming-tweets",
                        headers: { 'client-id': clientId },
                        pathParameters: [],
                        bodyParameters: [{ Id: 1, Name: "GET_TWEETS" }],
                        callback: successCallback,
                        errorCallback: errorCallback,
                        beforeSendCallback: beforeSendCallback,
                        completedCallback: completedCallback
                    }

                    return serverConnector.getInstance().callRESTServerMethod(settings);
                }
            };
        };

        return {

            getInstance: function () {

                if (!instance) {
                    instance = init();
                }

                return instance;
            }
        };

    })();

    window["clientApi"] = clientApi;

})();