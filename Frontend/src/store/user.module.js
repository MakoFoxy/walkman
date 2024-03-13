import axios from 'axios'
import {
    GET_PERMISSIONS, GET_USER_OBJECTS,
    LOGIN,
    LOGOUT, SET_USER_OBJECT_BY_INDEX
} from './actions.type'
import {
    SET_AUTH,
    PURGE_AUTH, SET_PERMISSIONS, SET_USER_OBJECTS, SET_USER_OBJECT_INDEX
} from './mutations.type'
import userHomePage from "@/helpers/UserHomePage";
import {AdminAccessObject, PartnerAccessToObject} from "@/helpers/permissions";

const state = {
    user: {
        permissions: [],
        objects: [],
    },
    token: '',
    selectedObjectIndex: 0,
}

const getters = {
    currentUser(state){
        return state.user
    },
    token(state){
        return state.token
    },
    isAdmin(state){
      if (state.user.permissions.includes(PartnerAccessToObject)){
          return false;
      }
      return true;
    },
    isPartner(state) {
        return state.user.permissions.includes(PartnerAccessToObject);
    },
    isObjectAdmin(state){
        return state.user.permissions.includes(AdminAccessObject);
    },
    selectedObjectIndex(state) {
        return state.selectedObjectIndex;
    },
}

const actions = {
    async [LOGIN](context, credentials){
        let response;
        try {
            response = await axios.post('auth', credentials)
        } catch (error) {
          throw new Error({
              response: {
                status: error.response.status,
                statusText:  error.response.status === 401 ? 'Неправильный логин или пароль' : 'Ошибка',
                data:  '',
              },
            toString: function (){
                return JSON.stringify(this)
            }
          })
        }
        axios.defaults.headers.common['Authorization'] = `Bearer ${response.data}`;
        this._vm.axios.defaults.headers.common['Authorization'] = `Bearer ${response.data}`;
        localStorage.setItem('token', response.data)
        context.commit(SET_AUTH, response.data)
    },
    async [GET_USER_OBJECTS](context) {
        let response;
        try {
            response = await axios.get('object/user');
            context.commit(SET_USER_OBJECTS, response.data.objects);
        } catch (e) {
            console.log(e);
        }
    },
    async [GET_PERMISSIONS](context){
        if (context.state.user.permissions.length === 0){
            const response = await axios.get('current-user/permissions');
            context.commit(SET_PERMISSIONS, response.data.permissions);

            if (
                response.data.permissions.includes(PartnerAccessToObject) ||
                response.data.permissions.includes(AdminAccessObject)
            ) {
                userHomePage.setDefaultPage('home-client');
            }
        }
    },
    [LOGOUT](context){
        localStorage.removeItem('token')
        context.commit(PURGE_AUTH)
    },
    [SET_USER_OBJECT_BY_INDEX](context, index) {
        if (index < 0){
            context.commit(SET_USER_OBJECT_INDEX, 0);
            return;
        }

        if (index >= context.state.user.objects.length) {
            context.commit(SET_USER_OBJECT_INDEX, context.state.user.objects.length - 1);
            return;
        }

        context.commit(SET_USER_OBJECT_INDEX, index);
    }
}

const mutations = {
    [SET_AUTH](state, token){
        state.token = token
    },
    [PURGE_AUTH](state){
        state.user = {
            permissions: [],
            objects: [],
        }
        state.token = ''
    },
    [SET_PERMISSIONS](state, permissions){
        state.user.permissions = permissions
    },
    [SET_USER_OBJECTS](state, userObjects) {
        state.user.objects = userObjects;
    },
    [SET_USER_OBJECT_INDEX](state, index) {
        this.state.user.selectedObjectIndex = index;
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};
