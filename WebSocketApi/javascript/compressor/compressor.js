var compressor = {

    _instance: null,


    getInstance: function () {

        if (!this._instance) {

            this._instance = {

                compress: function (data, callback, progressCallback) {

                    LZMA.compress(data, 1, callback, progressCallback);
                },

                decompress: function (compressedData, callBack, progressCallback) {

                    LZMA.decompress(compressedData, callBack, progressCallback);
                }
            }
        }

        return this._instance;
    }
}
