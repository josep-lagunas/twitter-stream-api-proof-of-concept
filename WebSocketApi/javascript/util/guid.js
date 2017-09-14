/**
 * Created by Josep on 22/1/15.
 */
(function() {
    var guid = (function () {

        var instance;

        function init() {

            function s4() {
                return Math.floor((1 + Math.random()) * 0x10000)
                    .toString(16)
                    .substring(1);
            }

            return function () {
                return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
                    s4() + '-' + s4() + s4() + s4();
            };
        }

        if (instance === undefined){
            instance = init();
        }

        return instance;

    })();

    window['guid'] = {

        createGUID : guid
    };

})();