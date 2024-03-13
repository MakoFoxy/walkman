import axios from 'axios'
import {
    SET_ADVERTS,
    CHANGE_ADVERTS_LOADING_STATE
 } from './mutations.type'

 import {
    GET_ADVERTS
 } from './actions.type'

const state = {
    adverts: [],
    advertsPage: 1,
    advertsPerPage: 0,
    advertsTotalItems: 0,
    advertsIsLoading: false
}

const getters = {
    adverts(state){
        return state.adverts.map(a => {
            return {...a, objects: a.objects.map(ao => ao.name)};
        })
    }
}

const actions = {
    async [GET_ADVERTS](context, pagination){
       context.commit(CHANGE_ADVERTS_LOADING_STATE, true)
        let url = `adverts?page=${pagination.page}&itemsPerPage=${pagination.itemsPerPage}`;

       if (pagination.objectId != null) {
           url += `&objectId=${pagination.objectId}`;
       }

       const response = await axios.get(url)
       context.commit(SET_ADVERTS, response.data)
       context.commit(CHANGE_ADVERTS_LOADING_STATE, false)
    }
}

const mutations = {
    [SET_ADVERTS](state, paginationResult){
        state.adverts = paginationResult.result
        state.advertsPage = paginationResult.page
        state.advertsTotalItems = paginationResult.totalItems
        state.advertsPerPage = paginationResult.itemsPerPage
    },
    [CHANGE_ADVERTS_LOADING_STATE](state, isLoading){
        state.advertsIsLoading = isLoading
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};
