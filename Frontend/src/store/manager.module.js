import {
    SET_MANAGER,
    CHANGE_MANAGER_LOADING_STATE
 } from './mutations.type'

 import {
    GET_MANAGERS
 } from './actions.type'

const state = {
    managers: [],
    managerPage: 1,
    managerPerPage: 0,
    managerTotalItems: 0,
    managersIsLoading: false
}

const getters = {
    manager(state){
        return state.manager
    }
}

const actions = {
    async [GET_MANAGERS](context, pagination){
        context.commit(CHANGE_MANAGER_LOADING_STATE, true)
        const response = await this._vm.axios.get(`managers?page=${pagination.page}&itemsPerPage=${pagination.itemsPerPage}`)
        context.commit(SET_MANAGER, response.data)
        context.commit(CHANGE_MANAGER_LOADING_STATE, false)
    }
}

const mutations = {
    [SET_MANAGER](state, paginationResult){
        state.managers = paginationResult.result
        state.managerPage = paginationResult.page
        state.managerTotalItems = paginationResult.totalItems
        state.managerPerPage = paginationResult.itemsPerPage
    },
    [CHANGE_MANAGER_LOADING_STATE](state, isLoading){
        state.managersIsLoading = isLoading
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};