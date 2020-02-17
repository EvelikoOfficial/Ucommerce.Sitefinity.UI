﻿import { initializeComponent } from "../functions/init";
import checkoutNavigation from "../components/checkout-navigation";
import inputField from "../components/input-field";
import store from '../store';

import { mapState } from 'vuex';

initializeComponent("address-widget", initCart);

function initCart(rootElement) {
    new Vue({
        el: '#' + rootElement.id,
        store,
        data: {
            model: null
        },
        computed: {
            ...mapState([
                'triggerSubmit'
            ])
        },
        watch: {
            triggerSubmit: function () {
                this.submit(null, false, (success) => {
                    if (success) {
                        this.$store.dispatch('widgetSubmitted');
                    }
                });
            }
        },
        components: {
            checkoutNavigation,
            inputField
        },
        methods: {
            submit: function (fieldName, doNotHighlight, callback) {
                var fields = this.$el.querySelectorAll('input[name], select[name]');
                var requestData = {};
                var store = this.$store;

                if (!callback) {
                    callback = function () { };
                }

                for (var field of fields) {
                    if (field.type == 'checkbox' || field.type == 'radio') {
                        requestData[field.name] = field.checked;
                    }
                    else {
                        requestData[field.name] = field.value;
                    }
                }

                this.$http.post(location.href + '/uc/checkout/address', requestData).then((response) => {
                    if (response.data) {
                        var data = response.data;

                        store.commit('update');

                        if (data.Status) {
                            if (fieldName) {
                                if (data.Data.errors && data.Data.errors[fieldName].length) {
                                    callback(false, data.Data.errors[fieldName][0]);
                                }
                                else {
                                    callback(true, '');
                                }
                            }
                            else if (data.Status == 'success') {
                                callback(true, '');
                            }
                            else {
                                if (!doNotHighlight) {
                                    this.highlightFields(data.Data.errors);
                                }

                                callback(false, data.Message);
                            }
                        }
                        else {
                            console.log("Unhandled exception");
                            callback(false, '');
                        }
                    }
                });
            },
            continueFn: function (callback) {
                this.submit(null, false, callback);
            },
            highlightFields: function (errors) {
                if (!errors || errors.length == 0) {
                    return;
                }

                for (var fieldName in errors) {
                    for (var input of this.$children) {
                        if (input.inputName == fieldName) {
                            input.errorMessage = errors[fieldName][0];
                        }
                    }
                }
            },
            handleIsShippingAddressDifferent: function () {
                setTimeout(() => {
                    this.submit(null, true);
                }, 500);
            }
        },
        created: function () {
            this.$store.commit('vuecreated', 'address');

            this.$http.get(location.href + '/uc/checkout/address', {}).then((response) => {
                if (response.data &&
                    response.data.Status &&
                    response.data.Status == 'success' &&
                    response.data.Data && response.data.Data.data) {

                    this.model = response.data.Data.data;
                }
                else {
                    this.model = null;
                }
            });
        }
    });
}

