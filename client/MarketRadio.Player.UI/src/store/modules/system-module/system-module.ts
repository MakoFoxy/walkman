/* eslint-disable  no-return-assign */

import {
  Module, VuexModule, Mutation, Action,
} from 'vuex-module-decorators';
import webSocketService from '@/shared/services/WebSocketService';

@Module({ name: 'SystemModule' })
export default class SystemModule extends VuexModule {
  _isOnline = false

  _version = ''

  _currentWindow = ''

  get currentWindow() {
    return this._currentWindow;
  }

  get isOnline() {
    return this._isOnline;
  }

  get version() {
    return this._version;
  }

  @Mutation
  setCurrentWindow(window: string) {
    this._currentWindow = window;
  }

  @Mutation
  networkStateChanged(isOnline: boolean) {
    this._isOnline = isOnline;
  }

  @Mutation
  setVersion(version: string) {
    this._version = version;
  }

  @Action
  async currentWindowChanged(window: string) {
    this.context.commit('setCurrentWindow', window);
  }

  @Action
  async connectionLost() {
    this.context.commit('networkStateChanged', false);
  }

  @Action
  async connectionRestored() {
    this.context.commit('networkStateChanged', true);
  }

  @Action
  async updateVersion(version: string) {
    this.context.commit('setVersion', version);
  }

  @Action
  async subscribeToBackend() {
    webSocketService.on('playlist-updating', () => {
      this.context.commit('setPlaylistUpdating', true);
    });
    webSocketService.on('playlist-update-finished', () => {
      this.context.commit('setPlaylistUpdating', false);
    });
  }
}
