import {
    SET_ERROR,
    SET_ROWS_PER_PAGE_IN_TABLE,
    CLEAR_ERROR
 } from './mutations.type'

 import {
    REGISTER_ERROR,
    INITIALIZE
 } from './actions.type'

const state = {
    error: {},
    hasError: false,
    rowPerPageInTable: 30
}

const getters = {
    hasError(state){
        return state.hasError
    },
    error(state){
        return state.error
    },
    rowPerPageInTable(state){
        return state.rowPerPageInTable
    }
}

const actions = {
    [INITIALIZE](context){
        let rowsCount = localStorage.getItem('rowsCount')

        if (!rowsCount){
            context.commit(SET_ROWS_PER_PAGE_IN_TABLE, 30)
        } else {
            context.commit(SET_ROWS_PER_PAGE_IN_TABLE, rowsCount)
        }
    },
    [REGISTER_ERROR](context, error){
        context.commit(SET_ERROR, {
            code : error.response.status,
            text:  error.response.statusText,
            data:  error.response.data,
            dateTime: new Date().toUTCString()
        })
    }
}

const mutations = {
    [SET_ERROR](state, error){
        const oldErrors = localStorage.getItem('errors')
        let errors = []

        if (oldErrors){
            errors = JSON.parse(oldErrors)
        }

        errors.push(error)
        localStorage.setItem('errors', JSON.stringify(errors))

        state.hasError = true
        state.error = error
    },
    [CLEAR_ERROR](state){
        state.hasError = false
        state.error = {}
    },
    [SET_ROWS_PER_PAGE_IN_TABLE](state, rowsCount){
        state.rowPerPageInTable = rowsCount
        localStorage.setItem('rowsCount', rowsCount)
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};