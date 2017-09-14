var cacheControler = {
    _instance: null,
    getInstance: function () {

        if (!this._instance) {

            this._instance = {
                
                isAvailable: (sessionStorage != undefined),
               
                proxy: {

                    exists: function (operationName, parameters) {

                        var key = (operationName + "_" + parameters.join('_')).toLowerCase();

                        return (sessionStorage["proxy_" + key] != undefined);
                    },

                    addOrUpdate: function (operationName, parameters, data) {
                        try {

                            var key = (operationName + "_" + parameters.join('_')).toLowerCase();

                            compressor.getInstance().Compress(JSON.stringify(data), function (key) {

                                return function (data) {

                                    sessionStorage["proxy_" + key] = JSON.stringify(data);

                                }
                            }(key));

                        } catch (e) {
                            //throw new Error("Error adding/updating cache for key " + Id);
                        }
                    },

                    get: function (operationName, parameters, callback) {

                        if (!this.exists(operationName, parameters))
                            //throw new Error("No chart found with the associated key " + Id);
                            return undefined;

                        var key = (operationName + "_" + parameters.join('_')).toLowerCase();


                        var decompressedData = JSON.parse(sessionStorage["proxy_" + key]);


                        compressor.getInstance().decompress(decompressedData,
                               function (callback) {
                                   return function (data) {

                                       decompressedData = JSON.parse(data);

                                       if (callback != undefined) {
                                           callback(decompressedData);
                                       }

                                   }
                               }(callback));

                    },
                    getSynch: function (operationName, parameters) {

                        if (!this.exists(operationName, parameters))
                            //throw new Error("No chart found with the associated key " + Id);
                            return undefined;

                        var key = (operationName + "_" + parameters.join('_')).toLowerCase();


                        var decompressedData = JSON.parse(sessionStorage["proxy_" + key]);

                        var flag = false;
                        var decompressedData = null;

                        compressor.getInstance().decompress(decompressedData, function (data) {

                            decompressedData = JSON.parse(data);
                            flag = true;
                        });

                        var intl = setInterval(function () {

                            if (flag) {
                                clearInterval(intl);
                                return decompressedData;
                            }


                        }, 100);

                    },
                    remove: function (operationName, parameters) {

                        var key = (operationName + "_" + parameters.join('_')).toLowerCase();

                        //ignore when sessionStorage not available due browser version
                        if (!CacheControler.getInstance().isAvailable || !this.exists(Key))
                            return;

                        sessionStorage.removeItem("proxy_" + key);

                    }

                },


                clear: function () {

                    sessionStorage.clear();

                }

            }
        }

        return this._instance;

    }
}
