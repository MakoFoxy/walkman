import {
    START_LOADING,
    STOP_LOADING
 } from './mutations.type'

const state = {
    isLoading: false,
}

const getters = {
    isLoading(state){
        return state.user
    }
}

const actions = {}

const mutations = {
    [START_LOADING](state){
        state.isLoading = true
    },
    [STOP_LOADING](state){
        state.isLoading = false
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};