import Vue from 'vue'
import moment from 'moment'
import vueDebounce from 'vue-debounce'
import './plugins/axios'
import './plugins/vuetify'
import App from './App.vue'
import router from './router'
import store from './store/store'
import {INITIALIZE} from './store/actions.type'
import './css/style.css'
import vuetify from './plugins/vuetify';

moment.locale('ru')

Vue.use(vueDebounce, {
  defaultTime: '500ms',
  listenTo: 'input'
})

Vue.directive('can', function (el, binding) {
  if (store.getters.currentUser.permissions.indexOf(binding.value)) {
    el.remove();
  }
})

Vue.config.productionTip = false

new Vue({
  router,
  store,
  vuetify,
  render: h => h(App)
}).$mount('#app')


store.dispatch(INITIALIZE)