import axios from 'axios'
import moment from 'moment'
import {
    SET_OBJECTS,
    CHANGE_OBJECT_LOADING_STATE
 } from './mutations.type'

 import {
    GET_OBJECTS
 } from './actions.type'

const state = {
    objects: [],
    objectsPage: 1,
    objectsPerPage: 0,
    objectsTotalItems: 0,
    objectsIsLoading: false
}

const getters = {
    objects(state){
        return state.objects
    }
}

const actions = {
    async [GET_OBJECTS](context, pagination){
        context.commit(CHANGE_OBJECT_LOADING_STATE, true)
        const query = {...pagination, date: moment(pagination.date).format('YYYY-MM-DD')}
        const response = await axios.get('object', {
                params: {
                    ...query
            }
        })
        context.commit(SET_OBJECTS, response.data)
        context.commit(CHANGE_OBJECT_LOADING_STATE, false)
    }
}

const mutations = {
    [SET_OBJECTS](state, paginationResult){
        state.objects = paginationResult.result
        state.objectsPage = paginationResult.page
        state.objectsTotalItems = paginationResult.totalItems
        state.objectsPerPage = paginationResult.itemsPerPage
    },
    [CHANGE_OBJECT_LOADING_STATE](state, isLoading){
        state.objectsIsLoading = isLoading
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};