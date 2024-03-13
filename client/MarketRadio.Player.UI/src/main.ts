import Vue from 'vue';
import { getModule } from 'vuex-module-decorators';
import * as Sentry from '@sentry/browser';
import * as Integrations from '@sentry/integrations';
import webSocketService from '@/shared/services/WebSocketService';
import CommunicationModel from '@/shared/models/CommunicationModel';
import axios from 'axios';
import App from './App.vue';
import router from './router';
import store from './store';
import vuetify from './plugins/vuetify';
import './plugins/axios';
import onlineConnection from './shared/services/OnlineConnection';
import ObjectModule from './store/modules/object-module/object-module';
import PlaylistModule from './store/modules/playlist-module/playlist-module';
import Playlist from './shared/models/PlaylistModel';
import SystemModule from './store/modules/system-module/system-module';
import Initiator from './shared/services/Initiator';

(async () => {
  Vue.config.productionTip = false;

  const vue = new Vue({
    router,
    store,
    vuetify,
    render: (h) => h(App),
  });

  await onlineConnection.connect(async () => {
    const initiator = new Initiator();
    await initiator.init(store, onlineConnection, false);
  });

  const initiator = new Initiator();
  const fullInit = await initiator.init(vue.$store, onlineConnection, true);

  if (!fullInit) {
    vue.$mount('#app');
    vue.$router.push({ name: 'login' });
    return;
  }

  vue.$mount('#app');
  vue.$router.push({ name: 'login' });

  await axios.post(`/playlist/${getModule(PlaylistModule, store).playlist.id}/tracks/download`);
})();
