/**
 * Created by Josep on 22/1/15.
 */
(function() {
    var eventSubscribersBus = (function () {
        var instance;
        function init() {
            var subscribers = [];
            return {
                addSubscriber: function (subscriber) {
                    subscribers.push(subscriber);
                },
                removeSubscriber: function (id) {
                    if (subscribers !== undefined && subscribers.length > 0) {
                        var i = 0;
                        while (i < subscribers.length && subscribers[i].id !== id) {
                            i++;
                        }
                        if (i < subscribers.length) {
                            subscribers.splice(i, 1);
                        }
                    }
                },
                notifyEvent: function (subscrIds, value) {
                    if (subscribers !== undefined && subscribers.length > 0) {
                        var i = 0;
                        while (i < subscribers.length) {
                            if (subscrIds.indexOf(subscribers[i].id) > -1) {
                                subscribers[i].callback(value);
                            }
                            i++;
                        }
                        return subscribers[i];
                    }
                },
                getSubscribers: function(){
                    return subscribers;
                }
            }
        }
        if (instance === undefined) {
            instance = init();
        }
        return instance;
    })();
    window['eventsSubscribersBus'] = eventSubscribersBus;
})();





