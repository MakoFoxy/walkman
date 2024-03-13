import axios from 'axios'
import {
    SET_MUSIC,
    CHANGE_MUSIC_LOADING_STATE,
    SET_TRACK_IN_AUDIO
 } from './mutations.type'

 import {
    GET_MUSIC
 } from './actions.type'

const state = {
    music: [],
    musicPage: 1,
    musicPerPage: 0,
    musicTotalItems: 0,
    musicIsLoading: false
}

const getters = {
    music(state){
        return state.music
    }
}

const actions = {
    async [GET_MUSIC](context, pagination){
        context.commit(CHANGE_MUSIC_LOADING_STATE, true)
        const response = await axios.get(`music?page=${pagination.page}&itemsPerPage=${pagination.itemsPerPage}`)
        
        const music = response.data.result.map(t =>{ 
            return {
                ...t,
                isPlaying: false
            }
        })

        context.commit(SET_MUSIC, music)
        context.commit(CHANGE_MUSIC_LOADING_STATE, false)
    }
}

const mutations = {
    [SET_MUSIC](state, music){
        state.music = music
    },
    [CHANGE_MUSIC_LOADING_STATE](state, isLoading){
        state.musicIsLoading = isLoading
    },
    [SET_TRACK_IN_AUDIO](state, data){
        const audio = data.vue.$refs.audio
        const songPath = `${location.origin}/songs/${data.track.filePath}`
        
        state.music.filter(t => t !== data.track).forEach(t => t.isPlaying = false)
        
        if (decodeURI(songPath) === decodeURI(audio.src) && data.track.isPlaying){
            data.track.isPlaying = false
            audio.pause()
            return
        }
        if (decodeURI(songPath) === decodeURI(audio.src)){
            audio.play()
            data.track.isPlaying = true
        }else{
            audio.src = `/songs/${data.track.filePath}`
            audio.play()
            data.track.isPlaying = true
        }
    }
}

export default {
    state,
    actions,
    mutations,
    getters
};