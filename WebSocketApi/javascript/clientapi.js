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
            serverConnector.setBaseURL("http://localhost:51500");
            //Singleton

            //Private Methods and Variables goes here

            var isStreamingLiveBets = false;

            return {

                startTweetsLiveStreaming: function (isAsync, interval, successCallback, errorCallback,
                    beforeSendCallback, completedCallback, onStreamMessage) {

                    var settings = {
                        verb: "POST",
                        async: isAsync,
                        method: "api/start-streaming",
                        pathParameters: [],
                        callback: successCallback,
                        errorCallback: errorCallback,
                        beforeSendCallback: beforeSendCallback,
                        completedCallback: completedCallback
                    };

                    if (isStreamingLiveBets === true) {
                        throw Error("Already streaming...");
                    }

                    isStreamingLiveBets = true;

                    var socket = io("http://localhost:51500/api/start-streaming");

                    socket.on('connect', function () { });
                    socket.on('bets::liveStreaming', function (onStreamMessage) {
                        return function (data) {
                            if (onStreamMessage) {
                                onStreamMessage(data);
                            }
                        }
                    }(onStreamMessage));

                    socket.on('disconnect', function () { });

                    serverConnector.getInstance().callRESTServerMethod(settings);
                },
                stopTweetsLiveStreaming: function (isAsync, successCallback, errorCallback,
                    beforeSendCallback, completedCallback) {
                    var settings = {
                        verb: "POST",
                        async: isAsync,
                        method: "api/stop-streaming",
                        pathParameters: [],
                        callback: successCallback,
                        errorCallback: errorCallback,
                        beforeSendCallback: beforeSendCallback,
                        completedCallback: completedCallback
                    };

                    isStreamingLiveBets = false;

                    serverConnector.getInstance().callRESTServerMethod(settings);
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


