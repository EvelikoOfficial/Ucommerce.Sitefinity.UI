﻿import { initializeComponent } from "../functions/init";
import checkoutNavigation from "../components/checkout-navigation";
import store from '../store';

import { mapState } from 'vuex';

initializeComponent("shipping-widget", initCart);

function initCart(rootElement) {
    new Vue({
        el: '#' + rootElement.id,
        store,
        data: {
            model: null
        },
        computed: {
            ...mapState([
                'triggerSubmit',
                'updateIteration'
            ]),
        },
        watch: {
            triggerSubmit: function () {
                this.submit((success) => {
                    if (success) {
                        this.$store.dispatch('widgetSubmitted');
                    }
                });
            },
            updateIteration: function () {
                this.fetchData();
            }
        },
        components: {
            checkoutNavigation
        },
        methods: {
            fetchData: function () {
                this.$http.get(location.href + '/uc/checkout/shipping', {}).then((response) => {
                    if (response.data &&
                        response.data.Status &&
                        response.data.Status == 'success' &&
                        response.data.Data && response.data.Data.data) {

                        this.model = response.data.Data.data;
                        this.selectDefalut();
                    }
                    else {
                        this.model = null;
                    }
                });
            },
            submit: function (callback) {
                var fields = this.$el.querySelectorAll('input[name]');
                var requestData = {};

                if (!callback) {
                    callback = function () { };
                }

                for (var field of fields) {
                    if (field.type == 'radio' && field.checked) {
                        requestData[field.name] = field.value;
                    }
                }

                this.$http.post(location.href + '/uc/checkout/shipping', requestData).then((response) => {
                    if (response.data) {
                        var data = response.data;

                        if (data.Status) {
                            if (data.Status == 'success') {
                                callback(true, '');
                            }
                            else {
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
            selectDefalut: function () {
                var methodIsAvaialle = () => {
                    for (var method of this.model.AvailableShippingMethods) {
                        if (method.Value == this.model.SelectedShippingMethodId) {
                            return true;
                        }
                    }

                    return false;
                }

                if (!methodIsAvaialle() && this.model.AvailableShippingMethods.length) {
                    this.model.SelectedShippingMethodId = this.model.AvailableShippingMethods[0].Value;
                }
            }
        },
        created: function () {
            this.$store.commit('vuecreated', 'shipping');

            this.fetchData();
        }
    });
}

