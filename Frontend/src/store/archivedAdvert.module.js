import axios from 'axios'
import {
    SET_ARCHIVED_ADVERTS,
    CHANGE_ARCHIVED_ADVERTS_LOADING_STATE
 } from './mutations.type'

 import {
    GET_ARCHIVED_ADVERTS,
    SEND_ADVERT_IN_ARCHIVE,
    DELETE_ADVERT_FROM_ARCHIVE,
    GET_ADVERTS
 } from './actions.type'

const state = {
    archivedAdverts: [],
    archivedAdvertsPage: 1,
    archivedAdvertsPerPage: 0,
    advertsTotalItems: 0,
    archivedAdvertsIsLoading: false
}

const getters = {
    archivedAverts(state){
        return state.archivedAdverts
    }
}

const actions = {
    async [GET_ARCHIVED_ADVERTS](context, pagination){
        context.commit(CHANGE_ARCHIVED_ADVERTS_LOADING_STATE, true)
        const response = await axios.get(`archived-adverts?page=${pagination.page}&itemsPerPage=${pagination.itemsPerPage}`)
        context.commit(SET_ARCHIVED_ADVERTS, response.data)
        context.commit(CHANGE_ARCHIVED_ADVERTS_LOADING_STATE, false)
    },
    async [SEND_ADVERT_IN_ARCHIVE](context, advertId){
        await axios.post('archived-adverts', JSON.stringify(advertId))
        const advertsPromise = context.dispatch(GET_ADVERTS, {page: context.rootState.advert.advertsPage, itemsPerPage: context.rootState.advert.advertsPerPage})
        const archivedAdvertsPromise = context.dispatch(GET_ARCHIVED_ADVERTS, {page: state.archivedAdvertsPage, itemsPerPage: state.archivedAdvertsPerPage})

        await Promise.all([advertsPromise, archivedAdvertsPromise])
    },
    async [DELETE_ADVERT_FROM_ARCHIVE](context, advertId){
        await axios.delete('archived-adverts',  {
            data: JSON.stringify(advertId)
        })
        const advertsPromise = context.dispatch(GET_ADVERTS, {page: context.rootState.advert.advertsPage, itemsPerPage: context.rootState.advert.advertsPerPage})
        const archivedAdvertsPromise = context.dispatch(GET_ARCHIVED_ADVERTS, {page: state.archivedAdvertsPage, itemsPerPage: state.archivedAdvertsPerPage})

        await Promise.all([advertsPromise, archivedAdvertsPromise])
    }
}

const mutations = {
    [SET_ARCHIVED_ADVERTS](state, paginationResult){
        state.archivedAdverts = paginationResult.result
        state.archivedAdvertsPage = paginationResult.page
        state.advertsTotalItems = paginationResult.totalItems
        state.archivedAdvertsPerPage = paginationResult.itemsPerPage
    },
    [CHANGE_ARCHIVED_ADVERTS_LOADING_STATE](state, isLoading){
        state.archivedAdvertsIsLoading = isLoading
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};