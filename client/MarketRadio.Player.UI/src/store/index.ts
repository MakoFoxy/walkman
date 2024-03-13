import Vue from 'vue';
import Vuex from 'vuex';
import playlistModule from './modules/playlist-module/playlist-module';
import objectModule from './modules/object-module/object-module';
import systemModule from './modules/system-module/system-module';
import userModule from './modules/user-module/user-module';

Vue.use(Vuex);

export default new Vuex.Store({
  modules: {
    objectModule,
    playlistModule,
    systemModule,
    userModule,
  },
  strict: process.env.NODE_ENV !== 'production',
});
