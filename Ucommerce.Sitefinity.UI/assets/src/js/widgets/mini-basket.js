﻿import { initializeComponent } from "../functions/init";
import store from '../store';

import { mapState } from 'vuex';

initializeComponent("mini-basket", initCart);;

function initCart(rootElement) {
    new Vue({
        el: '#' + rootElement.id,
        store,
        props: ['purl'],
        data: {
            model: null
        },
        computed: {
            ...mapState([
                'updateIteration'
            ])
        },
        watch: {
            updateIteration: function() {
                this.fetchData();
            }
        },
        methods: {
            fetchData: function () {
                this.$http.get(this.purl + '/uc/checkout/mini-basket', {}).then((response) => {
                    if (response.data &&
                        response.data.Status &&
                        response.data.Status == 'success' &&
                        response.data.Data &&
                        response.data.Data.data) {

                        this.model = response.data.Data.data;
                    } else {
                        this.model = null;
                    }
                });
            }
        },
        created: function () {
            var scriptElement = rootElement.querySelector('script[purl]');
            this.purl = scriptElement === null ? [] : JSON.parse(scriptElement.innerHTML).purl;
            this.fetchData();
        }
    });
}