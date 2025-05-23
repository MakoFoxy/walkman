import Vue from 'vue';
import VueCompositionAPI from '@vue/composition-api';
import App from './App.vue';
import router from './router';
import vuetify from './plugins/vuetify';

Vue.config.productionTip = false;
Vue.use(VueCompositionAPI);

const vue = new Vue({
  router,
  vuetify,
  render: h => {
    return h(App);
  }
});
vue.$mount('#app');
