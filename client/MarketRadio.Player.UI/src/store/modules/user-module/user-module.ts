import {
  Action, Module, Mutation, VuexModule,
} from 'vuex-module-decorators';
import CurrentUserInfo from '@/store/modules/user-module/CurrentUserInfo';

@Module({ name: 'UserModule' })
export default class UserModule extends VuexModule {
  _currentUserInfo = new CurrentUserInfo()

  get currentUserInfo() {
    return this._currentUserInfo;
  }

  @Mutation
  currentUserInfoUpdated(currentUserInfo: CurrentUserInfo) {
    this._currentUserInfo = currentUserInfo;
  }

  @Action
  async setCurrentUserInfo() {
    const response = await fetch('/api/current-user');
    const currentUserInfo = await response.json() as CurrentUserInfo;
    this.context.commit('currentUserInfoUpdated', currentUserInfo);
  }
}
