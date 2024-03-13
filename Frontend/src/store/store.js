import Vue from 'vue'
import Vuex from 'vuex'
import user from './user.module'
import loading from './loading.module'
import system from './system.modlue'
import object from './object.module'
import advert from './advert.module'
import archivedAdvert from './archivedAdvert.module'
import music from './music.module'
import client from './client.module'
import manager from './manager.module'

Vue.use(Vuex)

export default new Vuex.Store({
  modules: {
    user,
    loading,
    system,
    object,
    advert,
    archivedAdvert,
    music,
    client,
    manager
  }
})
