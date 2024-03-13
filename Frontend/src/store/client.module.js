import {
    SET_CLIENT,
    CHANGE_CLIENT_LOADING_STATE
 } from './mutations.type'

 import {
    GET_CLIENTS
 } from './actions.type'

const state = {
    clients: [],
    clientPage: 1,
    clientPerPage: 0,
    clientTotalItems: 0,
    clientsIsLoading: false
}

const getters = {
    client(state){
        return state.client
    }
}

const actions = {
    async [GET_CLIENTS](context, pagination){
        context.commit(CHANGE_CLIENT_LOADING_STATE, true)
        const response = await this._vm.axios.get(`organizations?page=${pagination.page}&itemsPerPage=${pagination.itemsPerPage}`)
        context.commit(SET_CLIENT, response.data)
        context.commit(CHANGE_CLIENT_LOADING_STATE, false)
    }
}

const mutations = {
    [SET_CLIENT](state, paginationResult){
        state.clients = paginationResult.result
        state.clientPage = paginationResult.page
        state.clientTotalItems = paginationResult.totalItems
        state.clientPerPage = paginationResult.itemsPerPage
    },
    [CHANGE_CLIENT_LOADING_STATE](state, isLoading){
        state.clientsIsLoading = isLoading
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};