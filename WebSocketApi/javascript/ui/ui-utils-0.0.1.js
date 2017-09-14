/**
 * Created by Josep on 13/1/15.
 **/

(function () {

    var modalWrapper = function (parameters) {
        var modal = function (parameters) {

            var title = '';
            var buttons = [];
            var addContentCallback;
            var showCallback;
            var shownCallback;
            var hideCallback;
            var hiddenCallback;
            var contentHeight;

            if (parameters) {
                title = parameters.title ? parameters.title : '';
                buttons = parameters.buttons ? parameters.buttons : [];
                addContentCallback = parameters.addContentCallback;
                contentHeight = parameters.contentHeight ? parameters.contentHeight : false;
                if (parameters.events) {
                    showCallback = parameters.events.showCallback;
                    shownCallback = parameters.events.shownCallback;
                    hideCallback = parameters.events.hide;
                    hiddenCallback = parameters.events.hidden;
                }
            }

            var item = "" +
                "<div id='modal' class='modal fade' tabindex='-1' role='dialog' aria-labelledby='myModalLabel' aria-hidden='true'>" +
                "<div class='modal-dialog'>" +
                "<div class='modal-content'>" +
                "<div class='modal-header'>" +
                "<h3 id='modal-title'>" + title + "</h3>" +
                "</div>" +
                "<div class='modal-body'" + (contentHeight ? "style='max-height:100%;'" : "") + "><!-- content goes here once rendered -->" +
                "<div id='modal-container-wrapper'></div></div>" +
                "<div class='modal-footer'>";
            buttons.forEach(function (button) {

                var forceClose = "";
                if (button.forceModalClose) {
                    forceClose = "data-dismiss='modal";
                }
                item += "<button data-button-id='" + button.id + "' class='btn' " + forceClose + "' aria-hidden='true'>" + button.text + "</button>";
            });
            item += "</div>" +
            "</div>" +
            "</div>" +
            "</div>";

            var menu = $(item).appendTo(document.body);
            //attach button actions
            buttons.forEach(function (button) {
                $(menu).find("[data-button-id='" + button.id + "']").bind("click", function (bodyContent) {
                    return function () {

                        button.callback(bodyContent);

                    }

                }($(menu).find("#modal-container-wrapper")));
            });

            //add user content in modal
            if (addContentCallback) {
                var container = $(menu).find("#modal-container-wrapper");
                addContentCallback(container);
            }

            //show event
            if (showCallback) {
                $(menu).on('show.bs.modal', function (showCallback, container) {
                    return function (e) {
                            showCallback(e);
                    }
                }(showCallback, $(menu).find(".modal-body")));
            }
            if (shownCallback) {
                //shown event
                $(menu).on('shown.bs.modal', function (shownCallback, container) {
                    return function (e) {
                        shownCallback(e);
                    }
                }(shownCallback, $(menu).find(".modal-body")));
            }
            if (hideCallback) {
                //hide event
                $(menu).on('hide.bs.modal', function (hideCallback, container) {
                    return function (e) {
                        hideCallback(e);
                    }
                }(hideCallback, $(menu).find(".modal-body")));
            }
            if (hiddenCallback) {
                //hidden event
                $(menu).on('hidden.bs.modal', function (hiddenCallback, container) {
                    return function (e) {
                        //every time modal is close it's removed from DOM so
                        //user has no need to close.
                        $(menu).remove();
                        hiddenCallback(e);
                    }
                }(hiddenCallback, menu));
            }

            $(menu).modal({keyboard: false, backdrop: 'static'});

            return menu;
        };

        return new modal(parameters);
    };

    var tabsWrapper = function(parameters) {

        this.id;

        var htmlInstantContent;

        var parameters;

        var tab = function(parameters) {

            this.id = parameters.id;

            if (!parameters.container) {
                throw new Error("parameters.container (doc element) is required.");
            }

            var container = parameters.container;

            var elem = "";
            if (parameters.title) {
                elem += "<h2>" + parameters.title + "</h2>";
            }

            if (!parameters.tabsDefinition) {
                throw new Error("No tabsDefinition element found in parameters. Parameters example " +
                    "{id: 'unique-id-for-tabs-instance'," +
                    "  title: 'main tab title'," +
                    "  tabsDefinition: [{ idTab: 'id', " +
                    "tabText: 'tabTitle', " +
                    "active: true, " +
                    "contentRenderCallback: function(tabContentContainer){" +
                    "//content to render as a Tab content goes here" +
                    "},...]}");
            }


            elem += "<ul id='nav-tabs' class='nav nav-tabs'></ul>";
            elem += "<div id='tab-content' class='tab-content'></div>";

            $(container).append(elem);

            var tabsContainer = $(container).find("#nav-tabs");
            var tabsContentContainer = $(container).find("#tab-content")
            $.each(parameters.tabsDefinition, function (i, current) {
                //tab
                elem = "<li class='" + (current.active ? "active" : "") + "'><a data-toggle='tab' href='#" + current.idTab + "'>";
                elem += current.tabText;
                elem += "</a></li>";
                $(tabsContainer).append(elem);
                //content for tab
                elem = "<div id='" + current.idTab + "' class='tab-pane fade in " + (current.active ? "active" : "") + "'></div>";
                var currentTabContainer = $(elem).appendTo(tabsContentContainer);
                //call render content from client
                current.contentRenderCallback(currentTabContainer);
            });

            htmlInstantContent = $(container);

        };

        tab.prototype.destroyInstance = function() {
            $(htmlInstantContent).children().remove();
        };


        tab.prototype.removeTabContent = function(idTab) {

            var container = $(htmlInstantContent).find("#idTab");
            $(container).append(element).children().remove();

        };

        tab.prototype.setTabContent = function(idTab, element) {

            var container = $(htmlInstantContent).find("#idTab");
            $(container).append(element);

        }

        return new tab(parameters);

    };

    var navigationBarWrapper = function(parameters){

        this.id;

        var htmlInstantContent;

        var parameters;

        function navigationBarMenuItemFactory(menuItem, container) {

            var item = "";

            switch (menuItem.type) {

                case uiNavigationBarMenuItems.MENU:

                    item += "<li id='" + menuItem.id + "' class='dropdown' data-type='" + menuItem.type + "'>";
                    item +=     "<a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-expanded='false'>";
                    item +=         menuItem.text;
                    item +=         " <span class='caret'></span>";
                    item +=     "</a>";
                    item +=     "<ul class='dropdown-menu'>";
                    item +=     "</ul>";
                    item += "</li>";

                    var element = $(item).appendTo(container);

                    menuItem.subMenu.forEach(function(menuItem) {
                        navigationBarMenuItemFactory(menuItem, $(element).find(".dropdown-menu").first());
                    });

                    if (!menuItem.callbackOnClick) {

                        $(element).bind("click", function (evt) {

                            if ($(evt.currentTarget).hasClass("disabled")) {

                                // Avoid following the href location when clicking
                                evt.preventDefault();
                                // Avoid having the menu to close when clicking
                                evt.stopPropagation();
                            }

                        });

                    } else {
                        throw new Error("click callback not allowed in MENU type");
                    }

                    return;
                    break;

                case uiNavigationBarMenuItems.SUBMENU:

                    item += "<li id='" + menuItem.id + "' class='dropdown dropdown-submenu' data-type='" + menuItem.type + "'>";
                    item +=     "<a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-expanded='false'>";
                    item +=         menuItem.text;
                    item +=     "</a>";
                    item +=     "<ul class='dropdown-menu'>";
                    item +=     "</ul>";
                    item += "</li>";

                    var element = $(item).appendTo(container);

                    menuItem.subMenu.forEach(function(menuItem) {
                        navigationBarMenuItemFactory(menuItem, $(element).find(".dropdown-menu"));
                    });

                    if (!menuItem.callbackOnClick) {

                        $(element).bind("click", function (evt) {

                            if ($(evt.currentTarget).hasClass("disabled")) {

                                // Avoid following the href location when clicking
                                evt.preventDefault();
                                // Avoid having the menu to close when clicking
                                evt.stopPropagation();
                            }

                        });

                    } else {
                        throw new Error("click callback not allowed in SUBMENU type");
                    }

                    return;

                case uiNavigationBarMenuItems.SEPARATOR:

                    item += "<li id='" + menuItem.id + "' class='divider' data-type='" + menuItem.type + "'></li>";
                    break;

                case uiNavigationBarMenuItems.BUTTON:

                    item += "<li id='" + menuItem.id + "' data-type='" + menuItem.type + "'>";
                    item +=     "<button type='submit' class='btn btn-default'>Submit</button>";
                    item += "</li>";
                    break;

                case uiNavigationBarMenuItems.SEARCH:

                    item += "<li id='" + menuItem.id + "' style='max-width: 200px;' data-type='" + menuItem.type + "'>";
                    item +=     "<div class='input-group input-group-sm' style='padding:10px;'>";
                    item +=         "<input type='text' class='form-control' placeholder=''>";
                    item +=         "<span class='input-group-btn'>";
                    item +=             "<button class='btn btn-default btn-sm' type='button'>";
                    item +=                 menuItem.text;
                    item +=             "</button>";
                    item +=         "</span>";
                    item +=     "</div>";
                    item +=     "<div id='container' class='search-result-container' data-result-container='1' style='display: none;'/>";
                    item += "</li>";

                    var element = $(item).appendTo(container);

                    if (menuItem.callbackOnClick) {

                        $(element).bind("click", function(evt){

                            if ($(evt.target).is("button")){
                                return;
                            }

                            // Avoid following the href location when clicking
                            evt.preventDefault();
                            // Avoid having the menu to close when clicking
                            evt.stopPropagation();

                        });

                        $(element).find("button").bind("click", menuItem.callbackOnClick);

                    }

                    if (menuItem.callbackOnKeyPress) {

                        $(element).find("[type='text']").bind("keypress", function (evt) {

                            menuItem.callbackOnKeyPress(evt);

                        });

                    }

                    if (menuItem.callbackOnKeyDown) {

                        $(element).find("[type='text']").bind("keydown", function (evt) {

                            menuItem.callbackOnKeyDown(evt);

                        });

                    }

                    if (menuItem.callbackOnKeyUp) {

                        $(element).find("[type='text']").bind("keyup", function (evt) {

                            menuItem.callbackOnKeyUp(evt);

                        });

                    }

                    return;

                case uiNavigationBarMenuItems.ACTION:

                    item += "<li id='" + menuItem.id + "' data-type='" + menuItem.type + "'><a href='#'>" + menuItem.text + "</a></li>";
                    break;
            }

            var element = $(item).appendTo(container);

            if (menuItem.callbackOnClick) {

                $(element).bind("click", function(evt){

                    if (!$(evt.currentTarget).hasClass("disabled")){
                        menuItem.callbackOnClick(evt);
                    }else{
                        // Avoid following the href location when clicking
                        evt.preventDefault();
                        // Avoid having the menu to close when clicking
                        evt.stopPropagation();
                    }

                });

            } else {
                $(element).bind("click", function(evt) {

                    // Avoid following the href location when clicking
                    evt.preventDefault();
                    // Avoid having the menu to close when clicking
                    evt.stopPropagation();

                });
            }
        }

        var navigationBar = function (parameters) {

            /*
             * menuItem : { id: uniqueId, text: 'text', type: [FORM, BUTTON, TEXT, NON_NAV_LINK, SEPARATOR, DROPDOWN], enabled: [true|false] callbackOnClick : function(evt), subItems: [menuItem] }
             * //note: subItems only allowed in DROPDOWN type
             * { ico: src, submenu : [menuItem] }
             */
            var mainMenu = [];
            var container;

            if (parameters) {
                //if no container found assumes body
                container = parameters.container ? parameters.container : $(document.body);
                mainMenu = parameters.mainMenu ? parameters.mainMenu : undefined;
            }

            var uniqueId = guid.createGUID();

            this.id = parameters.id;

            var item = "";
            item += "<nav id='" + uniqueId +"' class='navbar navbar-default' style='margin-top: -51px; z-index:1000;'>";
            item +=     "<div class='container-fluid'>";

            if (parameters.header) {

                item += "<div class='navbar-header'>";
                item +=     "<a class='navbar-brand' href='#'>";
                item +=     "<img title='' alt='Brand' src='" + parameters.header.icoUrl + "')' class='nav-bar-shell'/>";
                item +=     "</a>";
                item += "</div>";

            }

            item +=         "<!-- Collect the nav links, forms, and other content for toggling -->";
            item +=         "<div class='collapse navbar-collapse' id='bs-example-navbar-collapse-1'>";
            item +=             "<ul id='mainMenucontainerLeft' class='nav navbar-nav navbar-left'>";
            item +=             "</ul>";
            item +=             "<ul id='mainMenucontainerRight' class='nav navbar-nav navbar-right'>";
            item +=             "</ul>";
            item +=         "</div><!-- /.navbar-collapse -->";
            item +=     "</div><!-- /.container-fluid -->";
            item += "</nav>";

            var navBar = $(item).appendTo(container);

            if (parameters.header) {
                if (parameters.header.callbackOnClick) {
                    $(navBar).find("img").css({"cursor": "pointer"});
                    $(navBar).find("img").bind("click", function(evt) {
                       parameters.header.callbackOnClick(evt);
                    });
                }
            }

            var container = $(navBar).find(".navbar-left");

            mainMenu.left.forEach(function(menuItem){

                navigationBarMenuItemFactory(menuItem, container);

            });

            container = $(navBar).find(".navbar-right");

            mainMenu.right.forEach(function(menuItem){

                navigationBarMenuItemFactory(menuItem, container);

            });

            //part of submenu hacking for bootstrap 3.2.3
            $(container).parent().find('ul.dropdown-menu [data-toggle=dropdown]').on('click', function(event) {
                // Avoid following the href location when clicking
                event.preventDefault();
                // Avoid having the menu to close when clicking
                event.stopPropagation();
                // If a menu is already open we close it
                //$('ul.dropdown-menu [data-toggle=dropdown]').parent().removeClass('open');
                // opening the one you clicked on only if is not already opened
                if ($(this).parent().hasClass('open')){
                    return;
                }

                if ($(this).parent().hasClass('disabled')){
                    return;
                }

                $(this).parent().addClass('open');

                var menu = $(this).parent().find("ul");
                var menupos = menu.offset();

                if ((menupos.left + menu.width()) + 30 > $(window).width()) {
                    var newpos = - menu.width();
                } else {
                    var newpos = $(this).parent().width();
                }
                menu.css({ left:newpos });

            });

            htmlInstantContent = navBar;

            parameters = parameters;

            $(navBar).animate({ 'margin-top': '0px' }, 250);

        };

        navigationBar.prototype.addMainMenuItem = function(menuItem){

            //by default set to left if no value informed
            menuItem.position = (menuItem.position) ? menuItem.position : "left";

            var container;

            if (menuItem.position !== 'left' && menuItem.position !== 'right'){
                throw new Error("unknown position");
            }else if(menuItem.position === 'left') {

                container = $(htmlInstantContent).find(".navbar-left");

            }else if(menuItem.position === 'right') {

                container = $(htmlInstantContent).find(".navbar-right");

            }

            navigationBarMenuItemFactory(menuItem,container);

        };

        navigationBar.prototype.addSubMenuItem = function(idParent, menuItem){

            var parent = $(htmlInstantContent).find("#" + idParent);

            if ($(parent).attr("data-type") !== 'MENU' && $(parent).attr("data-type") !== 'SUBMENU'){
                throw new Error("Unable to add menuItem to a non MENU or SUBMENU item");
            }

            var container = $(parent).find(".dropdown-menu");

            navigationBarMenuItemFactory(menuItem,container);

        };

        navigationBar.prototype.setImage = function(imageUrl){

            htmlInstantContent.find("img").attr("src", imageUrl);

        };

        navigationBar.prototype.disableMenuItem = function(idMenuItem){

            var element = htmlInstantContent.find("#" + idMenuItem);
            if (element) {
                element.removeClass("open");
                element.addClass("disabled");
                //this applies only for search type items
                element.find("[type='text']").attr("disabled", "disabled");
                element.find("[type='text']").val("");
                element.find("[type='button']").attr("disabled", "disabled");
                element.find("[data-result-container='1']").children().remove();
                element.find("[data-result-container='1']").hide();
            }

        };

        navigationBar.prototype.enableMenuItem = function(idMenuItem){

            var element = htmlInstantContent.find("#" + idMenuItem);
            element.removeClass("disabled");

            //this applies only for search type items
            if (element) {
                element.removeClass("open");
                element.addClass("disabled");
                //this applies only for search type items
                element.find("[type=\"text\"]").removeAttr("disabled");
                element.find("[type=\"button\"]").removeAttr("disabled");
            }
        };

        navigationBar.prototype.removeMenuItem = function(idMenuItem){

            htmlInstantContent.find("#" + idMenuItem).remove();

        };

        navigationBar.prototype.showWorkArea = function() {

            parameters.workArea.focuson();

        };

        navigationBar.prototype.hideWorkArea = function() {

            parameter.workArea.focusout();

        };

        navigationBar.prototype.renderSearchResults = function(idSearchMenuItem, results) {

            var element = htmlInstantContent.find("#" + idSearchMenuItem);
            //check if its a search button

            if (!element.find("[type=\"text\"]") || !element.find("[type=\"button\"]")) {
                throw new Error("Menu button MUST be a SEARCH button");
            }

            var container = element.find("[data-result-container='1']");

            $(container).children().remove();
            $(container).hide();

            if(results && results.length > 0) {

                var itemTemplate = getHtmlRenderTemplateForSearchButton(idSearchMenuItem);

                var regex = /{([^}])*}/gi;

                var match = regex.exec(itemTemplate.matchItemRenderTemplate);

                var parsedValues = [];

                while (match != null) {

                    var valueToReplace = match[0];

                    var properties = valueToReplace.replace(' ', '')
                        .replace('{', '').replace('}', '').split('.');

                    var parsedValue = {};
                    parsedValue.toReplace = match[0];
                    parsedValue.toExtractValuesPropertiesVector = properties;

                    parsedValues.push(parsedValue);

                    match = regex.exec(itemTemplate.matchItemRenderTemplate);
                }

                $.each(results, function (i, result) {

                    var itemFromTemplate = itemTemplate.matchItemRenderTemplate;

                    $.each(parsedValues, function (i, parsedValue) {

                        var value = result;

                        $.each(parsedValue.toExtractValuesPropertiesVector, function(j, currentProperty){
                            if (currentProperty.indexOf('[')<0) {
                                value = value[currentProperty];
                            }else {
                                var nameAux = currentProperty.substr(0,currentProperty.indexOf('['));
                                var index = Number(currentProperty.replace(nameAux,'')
                                    .trim().replace('[','').replace(']',''));
                                value = value[nameAux][index];
                            }
                        });

                        itemFromTemplate = itemFromTemplate.replace(parsedValue.toReplace, value);

                    });
                    //add item
                    var element = $(itemFromTemplate).appendTo(container);
                    //add data to item
                    $(element).data("info", result);

                    //add events
                    $.each(itemTemplate.matchItemRenderEvents.clickEvents, function(i, currentElement) {
                        var tagEventReceiver = $(element).find("#" + currentElement.id);
                        $(tagEventReceiver).css({ "cursor" : "pointer"});
                        $(tagEventReceiver).bind("click", function(callbackOnClick) {
                            return function(evt) {
                                callbackOnClick(evt);
                            }
                        }(currentElement.callback));
                    });
                });

                if (container.children().size() > 0) {
                    $(container).show();
                }


            }

        };

        function getElementByIdFromConfigParameters(id, element) {

            if (element && element.id && element.id === id) {
                return element;
            }

            if (element && element.type &&
                (element.type === "MENU" || element.type === "SUBMENU")) {
                getElementByIdFromConfigParameters(id, element.subMenu);
            }
        }

        function getHtmlRenderTemplateForSearchButton(idSearchButton) {

            var item;

            if (parameters.mainMenu.left) {
                $.each(parameters.mainMenu.left, function (i, current) {
                    item = getElementByIdFromConfigParameters(idSearchButton, current);
                });
                if (parameters.mainMenu.right && !item) {
                    $.each(parameters.mainMenu.right, function (i, current) {
                        item = getElementByIdFromConfigParameters(idSearchButton, current);
                    });
                }

                return item;
            }
        }

        navigationBar.prototype.destroyNavigationBar = function(){

            $(htmlInstantContent).remove();

        };

        navigationBar.prototype.getjQueryObject = function(){

            return htmlInstantContent;

        };

        return new navigationBar(parameters);

    };

    var workAreaWrapper = function(parameters) {

        this.id;

        var htmlInstantContent;

        var order;

        var rotation;
        var translation;

        var parameters;

        var workArea = function (parameters) {

            this.id = parameters.id;

            var container = $("#workareascarousel");

            var elem = "";
            elem += "<div id='" + this.id + "' class='workarea'></div>";

            htmlInstantContent = $(elem).appendTo(container);

            parameters = parameters;

        };

        workArea.prototype.getId = function () {

            return this.id;

        };

        workArea.prototype.getOrder = function() {

            return order;

        };

        workArea.prototype.setOrder = function(position) {

            order = position;

        };

        workArea.prototype.focuson = function () {

            $(htmlInstantContent).animate({ "left" : "0%"});

        };

        workArea.prototype.focusout = function () {

            $(htmlInstantContent).animate({ "left" : "-100%"});

        };

        workArea.prototype.getjQueryElement = function () {

            return htmlInstantContent;

        };

        workArea.prototype.getWidth = function () {

            return $(htmlInstantContent).css("width");

        };

        workArea.prototype.setRotation = function(deg){

            rotation = deg;

        };

        workArea.prototype.getRotation = function(){

            return rotation;

        };

        workArea.prototype.setTranslation = function(pixels){

            translation = pixels;

        };

        workArea.prototype.getTranslation = function(){

            return translation;

        };

        return new workArea(parameters);

    };



    window['utils'] = {

        specialEvents: {

            unbindMouseWheelEvent: function(element, callback) {

                if (element.removeEventListener) {
                    element.removeEventListener("mousewheel", callback, false);
                    element.removeEventListener("DOMMouseScroll", callback, false);
                }else if (element.detachEvent) {
                    element.detachEvent("onmousewheel", callback);
                }else {
                    element["onmousewheel"] = null;
                }
            },
            bindMouseWheelEvent: function (element, callback) {

                var onCallback = function (callback) {
                    return function (e) {
                        var e = window.event || e; //old IE support
                        //delta property doesn't exists originally but is set in order to
                        //normalize delta values in [-1|1] for cross-browser purpose
                        e.delta = Math.max(-1, Math.min(1, (e.wheelDelta || -e.detail)));
                        callback(e);
                    }
                }(callback);

                if (element.addEventListener) {
                    //IE9, Chrome, Safari, Opera
                    element.addEventListener("mousewheel", onCallback, false);
                    //firefox
                    element.addEventListener("DOMMouseScroll", onCallback, false);

                } else if (element.attachEvent) {
                    //IE 6/7/8
                    element.attachEvent("onmousewheel", onCallback);
                } else {
                    element["onmousewheel"] = onCallback;
                }
            }
        },

        modal: {

            showModal: modalWrapper,
            disableModalButton: function (idButton) {
                $("#modal").find(".modal-footer")
                    .find("[data-button-id = '" + idButton + "']")
                    .attr("disabled", "disabled");
            },
            enableModalButton: function (idButton) {

                $("#modal").find(".modal-footer")
                    .find("[data-button-id = '" + idButton + "']")
                    .removeAttr("disabled");
            },
            changeModalTitle: function (title) {
                $("#modal #modal-title").text(title);
            },
            forceCloseModal: function () {
                $("#modal").modal("hide");
            },
            resizeModalWidthInPixels: function (width) {

                if (!isNaN(width)) {
                    width = width.toString() + "px";
                } else if(width.indexOf("px") === -1){
                    throw new Error("accepted value for resize modal width sample [ 600, \"600px\"]");
                }
                $("#modal .modal-dialog").css({ width: width });
            }

        },
        navMenu: {

            navigationBarInstances: {},

            navigatorBars: {

                getNavigationBar: function (idNavigationBar) {

                    return window.utils.navMenu.navigationBarInstances[idNavigationBar];

                },
                destroyNavigationBar: function (idNavigationBar) {

                    window.utils.navMenu.navigationBarInstances[idNavigationBar].destroyNavigationBar();
                    window.utils.navMenu.navigationBarInstances[idNavigationBar] = null;
                    delete window.utils.navMenu.navigationBarInstances[idNavigationBar];

                },
                createNavigationBar: function (parameters) {

                    var navBar = navigationBarWrapper(parameters);

                    //add navigationBar reference to navBar hasMap
                    var instances = window.utils.navMenu.navigationBarInstances;

                    instances[navBar.id] = navBar;

                    return navBar;

                }

            }
        },

        tabs: {

            tabsInstances: {},

            createTabs: function(parameters) {

                if (!parameters || !parameters.id) {
                    throw new Error("parameters.id is required");
                }

                var tabs = tabsWrapper(parameters);
                var instances = window.utils.tabs.tabsInstances;

                instances[tabs.id] = tabs;

                return tabs;

            },
            getTabs: function(idTabs) {

                return window.utils.tabs.tabsInstances[idTabs];

            },
            destroyTabs: function(idTabs) {

                if (window.utils.tabs.tabsInstances[idTabs]) {
                    window.utils.tabs.tabsInstances[idTabs].destroyInstance();
                    window.utils.tabs.tabsInstances[idTabs] = null;
                    delete window.utils.tabs.tabsInstances[idTabs];
                }

            }

        },
        workArea: {

            rotateCarousel : function(currentTranslation, currentRotation,
                                      futureTranslation, futureRotation){

                var carousel = window.utils.workArea.workAreasCarousel.jQueryElement;

                var instancesCount = window.utils
                    .workArea.workAreasCarousel.workAreasInstances.length;

                if (instancesCount === 1) {
                    $(carousel).css(
                        {
                            "-webkit-transform":
                            "translateZ(-" + futureTranslation + "px) rotateY(0deg)"
                        }).css(
                        {
                            "transform":
                            "translateZ(-" + futureTranslation + "px) rotateY(0deg)"
                        }
                    );

                    return;
                }

                var distancePreRotation = 450 * instancesCount;

                //rotation once animation ended
                //window.utils.workArea.workAreasCarousel.currentCarouselRotation = futureRotation;

                $(carousel).bind('otransitionend transitionend webkitTransitionEnd', function (translation, rotation, distancePreRotation) {
                    return function () {

                        $(carousel).unbind();
                        $(carousel).bind('otransitionend transitionend webkitTransitionEnd', function (translation, rotation) {
                            return function () {
                                $(carousel).unbind();
                                $(carousel)
                                    .css({"-webkit-transform": "translateZ(-" + translation + "px) rotateY(" + rotation + "deg)"})
                                    .css({"transform": "translateZ(-" + translation + "px) rotateY(" + rotation + "deg)"});

                            }
                        }(translation, rotation));

                        $(carousel).css(
                            {"-webkit-transform": "translateZ(-" + distancePreRotation + "px) rotateY(" + rotation + "deg)"}
                        ).css(
                            {"transform": "translateZ(-" + distancePreRotation + "px) rotateY(" + rotation + "deg)"}
                        );
                    }
                }(futureTranslation, futureRotation, distancePreRotation));

                $(carousel).css(
                    {
                        "-webkit-transform":
                        "translateZ(-" + distancePreRotation + "px) rotateY(" + currentRotation + "deg)"
                    }).css(
                    {
                        "transform":
                        "translateZ(-" + distancePreRotation + "px) rotateY(" + currentRotation + "deg)"
                    }
                );

            },

            workAreasCarousel: undefined

            ,createWorkArea: function (parameters) {

                if (!parameters || !parameters.id) {
                    throw new Error("parameters.id is required");
                }

                if (!window.utils.workArea.workAreasCarousel) {

                    var container = $("#workareascontainer");
                    if ($(container).size() === 0) {
                        container = $("<div id='workareascontainer'>" +
                            "<div id='workareascarousel'></div></div>")
                            .appendTo($(document.body));

                        window.utils.workArea.workAreasCarousel = {
                            workAreasInstances: [],
                            currentWorkAreaShowed: -1,
                            currentWorkAreaIndex: -1,
                            currentCarouselRotation: 0,
                            angularDivisionPerWorkAreas: -1,
                            jQueryElement: $(container).find("#workareascarousel"),

                            allowMouseWheelRotation: function (allow) {

                                if (!window.utils.workArea.workAreasCarousel) {
                                    throw new Error("No carousel instance defined.");
                                }

                                var mouseWheelCallback = function (e) {

                                    if (window.utils.workArea.workAreasCarousel.rotationKeyDown) {
                                        if (e.delta > 0) {
                                            utils.workArea.workAreasCarousel.setFocusOnNext();
                                        } else {
                                            utils.workArea.workAreasCarousel.setFocusOnPrevious();
                                        }
                                    }
                                }

                                if (!allow) {
                                    window.utils.workArea.workAreasCarousel.rotationKeyDown = false;
                                    $(document).unbind("keydown").unbind("keyup");
                                    utils.specialEvents.unbindMouseWheelEvent(document,
                                        mouseWheelCallback);
                                } else {

                                    //bind alt key detection
                                    $(document).unbind("keydown");
                                    $(document).unbind("keyup");
                                    $(document).bind("keydown", function (e) {

                                        if (e.which === 37) {
                                            window.utils.workArea.workAreasCarousel.setFocusOnPrevious();
                                            return;
                                        } else if (e.which === 39) {
                                            window.utils.workArea.workAreasCarousel.setFocusOnNext();
                                            return;
                                        }

                                        if (e.which === 18) {
                                            window.utils.workArea.workAreasCarousel
                                                .rotationKeyDown = true;
                                        }


                                    }).bind("keyup", function (e) {
                                        if (e.which === 18) {
                                            window.utils.workArea.workAreasCarousel
                                                .rotationKeyDown = false;
                                        }
                                    });

                                    utils.specialEvents.unbindMouseWheelEvent(document,
                                        mouseWheelCallback);
                                    utils.specialEvents.bindMouseWheelEvent(document,
                                        mouseWheelCallback);
                                }
                            },

                            setFocusOnNext: function () {

                                var pointer = window.utils.workArea;

                                if (!pointer.workAreasCarousel) {
                                    throw new Error("No carousel defined.");
                                }

                                if (pointer.workAreasCarousel) {

                                    var workAreasCount =
                                        pointer.workAreasCarousel.workAreasInstances.length;
                                    pointer = pointer.workAreasCarousel.currentWorkAreaIndex;

                                    //there are instances but not pointed to anyone, probably first
                                    //setFocus after insert
                                    if (pointer < 0) {
                                        pointer++;
                                    }

                                    //get the current workarea pointed
                                    var currentWorkArea = window.utils.workArea.workAreasCarousel
                                        .workAreasInstances[pointer];
                                    //advance the pointer
                                    pointer = (pointer + 1) % workAreasCount;
                                    //fix the pointer
                                    window.utils.workArea.workAreasCarousel.currentWorkAreaIndex = pointer;
                                    //get the future workArea
                                    var workArea = window.utils.workArea.workAreasCarousel
                                        .workAreasInstances[pointer];
                                    //update the workArea showed name
                                    window.utils.workArea.workAreasCarousel
                                        .currentWorkAreaShowed = workArea.getId();

                                    var angularStep = window.utils.workArea.workAreasCarousel
                                        .angularDivisionPerWorkAreas;

                                    var currentCarouselRotation =
                                        window.utils.workArea.workAreasCarousel
                                            .currentCarouselRotation;

                                    var rotation = currentCarouselRotation - angularStep;

                                    window.utils.workArea.workAreasCarousel
                                        .currentCarouselRotation =
                                        rotation;

                                    window.utils.workArea
                                        .rotateCarousel(
                                        currentWorkArea.getTranslation()
                                        , currentCarouselRotation
                                        , workArea.getTranslation()
                                        , rotation
                                    );

                                    return workArea;
                                }

                            },
                            setFocusOnPrevious: function () {

                                var pointer = window.utils.workArea;

                                if (!pointer.workAreasCarousel) {
                                    throw new Error("No carousel defined.");
                                }

                                var workAreasCount =
                                    pointer.workAreasCarousel.workAreasInstances.length;

                                pointer = pointer.workAreasCarousel.currentWorkAreaIndex;

                                if (pointer < 0) {
                                    pointer = workAreasCount - 1;
                                }
                                //get the current workArea
                                var currentWorkArea = window.utils.workArea.workAreasCarousel
                                    .workAreasInstances[pointer];
                                //if we are pointing to first one, point to last
                                if (pointer === 0) {
                                    pointer = workAreasCount;
                                }
                                //update pointer
                                pointer = (pointer - 1) % workAreasCount;

                                //update pointer
                                window.utils.workArea.workAreasCarousel
                                    .currentWorkAreaIndex = pointer;

                                //get future workArea
                                var workArea = window.utils.workArea.workAreasCarousel
                                    .workAreasInstances[pointer];
                                window.utils.workArea.workAreasCarousel
                                    .currentWorkAreaShowed = workArea.getId();
                                //get angularStep to increase in one step the carousel movement
                                var angularStep = window.utils.workArea.workAreasCarousel
                                    .angularDivisionPerWorkAreas;
                                //get current carousel rotation angle
                                var currentCarouselRotation =
                                    window.utils.workArea.workAreasCarousel
                                        .currentCarouselRotation;
                                //increase carousel rotation in one step
                                var rotation = currentCarouselRotation + angularStep;
                                //update carousel rotation
                                window.utils.workArea.workAreasCarousel
                                    .currentCarouselRotation =
                                    rotation;

                                //perform rotation from current to next
                                window.utils.workArea
                                    .rotateCarousel(
                                    currentWorkArea.getTranslation()
                                    , currentCarouselRotation
                                    , workArea.getTranslation()
                                    , rotation
                                );

                                //return destination workArea
                                return workArea;

                            }
                        }
                    }
                }

                var workArea = workAreaWrapper(parameters);

                var instances = window.utils.workArea.workAreasCarousel.workAreasInstances;

                instances.push(workArea);

                //càlcul separació panels
                var separation = 360 / instances.length;
                var accSeparation = 0;

                window.utils.workArea.workAreasCarousel.angularDivisionPerWorkAreas = separation;

                var workAreaWidth = Number((workArea.getWidth().replace("px", "")));

                var tz = Math.round(( workAreaWidth / 2 ) /
                    Math.tan(Math.PI / instances.length));

                if (instances.length === 1) {
                    tz = workAreaWidth / 4.12; //4.12 is a calculated constant value
                }

                $.each(instances, function (i, current) {

                    current.getjQueryElement().css({
                        "-webkit-transform": "rotateY(" +
                        accSeparation.toString() + "deg) translateZ(" + tz + "px)"
                    }).css({
                            "transform": "rotateY(" +
                            accSeparation.toString() + "deg) translateZ(" + tz + "px)"
                        });

                    current.setOrder(i);
                    current.setRotation(accSeparation);
                    current.setTranslation(tz);
                    accSeparation += separation;

                });

                if (parameters.url != undefined) {
                    this.getWorkAreaContent(parameters.id).load(parameters.url);
                }

                //we need to recalculate carousel rotation points to set the current showed view
                //properly -> rotate to 0 deg position
                //and set de pointer pointing to the first workarea element
                window.utils.workArea.workAreasCarousel.currentCarouselRotation =
                    0;

                window.utils.workArea.workAreasCarousel.currentWorkAreaIndex = 0;
                window.utils.workArea.workAreasCarousel.currentWorkAreaShowed =
                   instances[0];

                this.setFocusOnWorkArea(parameters.id, true);

                return workArea;

            },
            getWorkAreaContent: function(id) {

                var carousel = window.utils.workArea.workAreasCarousel.jQueryElement;

                return $(carousel).find("#" + id);


            },
            setFocusOnWorkArea: function (id, forceFocus) {


                if (!forceFocus) {
                    forceFocus = false;
                }

                if (window.utils.workArea.workAreasCarousel.currentWorkAreaShowed === id
                    && !forceFocus) {
                    return;
                }

                var currentIndex = window.utils.workArea.workAreasCarousel.currentWorkAreaIndex;

                var workArea = window.utils.workArea.workAreasCarousel.setFocusOnPrevious();
                currentIndex++;
                if (workArea) {

                    while (workArea && workArea.getId() !== id) {
                        workArea = window.utils.workArea.workAreasCarousel.setFocusOnPrevious();
                        currentIndex++;
                    }
                    window.utils.workArea.workAreasCarousel.currentWorkAreaIndex = workArea.getOrder();
                    window.utils.workArea.workAreasCarousel.currentWorkAreaShowed = workArea.getId();
                }

            }

        }
    }

})();



