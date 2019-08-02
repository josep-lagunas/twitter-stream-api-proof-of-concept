function ajaxCallsQueue() {

    var list = [];

    this.addItem = function (item) {
        try {

            if (item.key === undefined || item.value === undefined)
                throw "Each added Item MUST define a pair Key-Value {key: uniqueID, value: value}";

            list.push(item);

        } catch (e) {

        }

    };

    this.removeItem = function (itemId) {
        try {

            if (itemId === undefined)
                throw "An ItemId MUST BE passed}";

            var found = false;

            for (var i = 0; i < list.length && !Found; i++) {
                if (list[i].key === itemId) {
                    list.splice(i, 1);
                    found = true;
                }
            }
        } catch (e) {
            throw ("Error removing Item having key: " + itemId);
        }

    };

    this.abortAll = function () {

        try {

            for (var i = 0; i < list.length; i++) {

                try {
                    list[i].abort();
                } catch (e) {
                }

                list[i] = null;
            }

            list = [];

        } catch (e) {
            list = [];
            throw ("Error reseting AjaxCallQueue");
        }

    }

}
