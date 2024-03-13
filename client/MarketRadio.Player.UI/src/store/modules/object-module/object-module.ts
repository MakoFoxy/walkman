/* eslint-disable  no-return-assign */
import {
  Module, VuexModule, Mutation, Action,
} from 'vuex-module-decorators';
import axios from 'axios';
import ObjectModel from '@/shared/models/ObjectModel';
import Settings from '@/shared/models/Settings';

@Module({ name: 'ObjectModule' })
export default class ObjectModule extends VuexModule {
  _object: ObjectModel | null = null

  get object() {
    return this._object;
  }

  @Mutation
  objectChanged(object: ObjectModel) {
    this._object = new ObjectModel({ ...object });
  }

  @Mutation
  settingsChanged(settings: Settings) {
    this._object!.settings = settings;
  }

  @Action
  async changeObject(object: ObjectModel) {
    this.context.commit('objectChanged', object);
  }

  @Action
  async updateObjectSettings(settings: Settings) {
    this.context.commit('settingsChanged', settings);
    await axios.post(`/client/${this.object!.id}/settings`, JSON.stringify(settings));
  }
}
