/* eslint-disable  no-return-assign */

import {
  Module, VuexModule, Mutation, Action,
} from 'vuex-module-decorators';
import Track from '@/shared/models/Track';
import Playlist from '@/shared/models/PlaylistModel';
import onlineConnection from '@/shared/services/OnlineConnection';

@Module({ name: 'PlaylistModule' })
export default class PlaylistModule extends VuexModule {
  _currentTrack: Track = new Track()

  _currentTrackStartTime: Date = new Date()

  _playlist: Playlist = new Playlist()

  _playlistLoaded = false

  _currentTrackChanging = false

  _errorInCurrentTrack = false

  _playlistUpdating = false

  get playlist() {
    return this._playlist;
  }

  get currentTrack() {
    return this._currentTrack;
  }

  get previousTrack() {
    const tracksBeforeCurrentTrack = this.tracks.filter((t) => t.playingDateTime.getTime() < this.currentTrack.playingDateTime.getTime());

    if (tracksBeforeCurrentTrack.length === 0) {
      return null;
    }

    return tracksBeforeCurrentTrack[tracksBeforeCurrentTrack.length - 1];
  }

  get nextTrack() {
    const tracksAfterCurrentTrack = this.tracks.filter((t) => t.playingDateTime.getTime() > this.currentTrack.playingDateTime.getTime());

    if (tracksAfterCurrentTrack.length === 0) {
      return null;
    }

    return tracksAfterCurrentTrack[0];
  }

  get tracks() {
    return [...this._playlist.tracks].sort((t1, t2) => t1.playingDateTime.getTime() - t2.playingDateTime.getTime());
  }

  get playlistLoaded() {
    return this._playlistLoaded;
  }

  get uniqueAdverts() {
    const uniqueAdverts = new Array<Track>();

    this._playlist.tracks.filter((t) => t.type === 'Advert').forEach((t) => {
      if (uniqueAdverts.find((a) => a.id === t.id) == null) {
        uniqueAdverts.push(t);
      }
    });

    return uniqueAdverts;
  }

  get uniqueMusic() {
    const uniqueMusic = new Array<Track>();

    this._playlist.tracks.filter((t) => t.type === 'Music').forEach((t) => {
      if (uniqueMusic.find((a) => a.id === t.id) == null) {
        uniqueMusic.push(t);
      }
    });

    return uniqueMusic;
  }

  get playlistUpdating() {
    return this._playlistUpdating;
  }

  @Mutation
  playlistChanged(playlist: Playlist) {
    const playlistFormatted = { ...playlist, date: new Date(playlist.date) };
    this._playlist = playlistFormatted;
  }

  @Mutation
  currentTrackChanged(track: Track) {
    this._currentTrack = track;
    this._currentTrackStartTime = new Date();
  }

  @Mutation
  trackInPlaylistAdded(track: Track) {
    track.playingDateTime = new Date(track.playingDateTime);
    if (this._playlist.tracks.find((t) => t.uniqueId === track.uniqueId) != null) {
      return;
    }
    this._playlist.tracks.push(track);
  }

  @Mutation
  setPlaylistLoaded() {
    this._playlistLoaded = true;
  }

  @Mutation
  currentTrackChanging(isChanging: boolean) {
    this._currentTrackChanging = isChanging;
  }

  @Mutation
  errorInCurrentTrackFired(errorFired: boolean) {
    this._errorInCurrentTrack = errorFired;
  }

  @Mutation
  setPlaylistUpdating(isUpdating: boolean) {
    this._playlistUpdating = isUpdating;
  }

  @Action
  async changePlaylist(playlist: Playlist): Promise<void> {
    playlist.tracks.forEach((t) => t.playingDateTime = new Date(t.playingDateTime));
    this.context.commit('playlistChanged', playlist);
    this.context.commit('setPlaylistLoaded');
  }

  @Action
  loadNextTrack() {
    if (this._currentTrack == null) {
      return;
    }

    this.context.commit('currentTrackChanging', true);

    this.context.commit('errorInCurrentTrackFired', false);
    const nextTrackIndex = this._playlist.tracks.indexOf(this._currentTrack) + 1;

    if (nextTrackIndex < this._playlist.tracks.length) {
      this.context.commit('currentTrackChanged', this._playlist.tracks[nextTrackIndex]);
    } else {
      // TODO По сути сюда заходить не должен убрать потом, только при ручном управлении
      this.context.commit('currentTrackChanged', this._playlist.tracks[0]);
    }

    this.context.commit('currentTrackChanging', false);
  }

  @Action
  addTrackInPlaylist(track: Track) {
    this.context.commit('trackInPlaylistAdded', track);
  }

  @Action
  async loadTrackByUniqueId(uniqueId: string) {
    this.context.commit('currentTrackChanging', true);

    if (this.context.getters!.currentTrack != null) {
      if (this.context.getters!.playlist.tracks[0] === this.context.getters!.currentTrack) {
        onlineConnection.sendPlaylistStarted();
      }
    }

    const currentTrack = this.context.getters!.tracks.find((t: Track) => t.uniqueId === uniqueId);
    this.context.commit('currentTrackChanged', currentTrack);
    this.context.commit('currentTrackChanging', false);
  }
}
