// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'playlist_store.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic

mixin _$PlaylistStore on _PlaylistStore, Store {
  Computed<int> _$musicInPlaylistComputed;

  @override
  int get musicInPlaylist =>
      (_$musicInPlaylistComputed ??= Computed<int>(() => super.musicInPlaylist,
              name: '_PlaylistStore.musicInPlaylist'))
          .value;
  Computed<int> _$advertInPlaylistComputed;

  @override
  int get advertInPlaylist => (_$advertInPlaylistComputed ??= Computed<int>(
          () => super.advertInPlaylist,
          name: '_PlaylistStore.advertInPlaylist'))
      .value;

  final _$playlistAtom = Atom(name: '_PlaylistStore.playlist');

  @override
  Playlist get playlist {
    _$playlistAtom.reportRead();
    return super.playlist;
  }

  @override
  set playlist(Playlist value) {
    _$playlistAtom.reportWrite(value, super.playlist, () {
      super.playlist = value;
    });
  }

  final _$currentTrackAtom = Atom(name: '_PlaylistStore.currentTrack');

  @override
  Track get currentTrack {
    _$currentTrackAtom.reportRead();
    return super.currentTrack;
  }

  @override
  set currentTrack(Track value) {
    _$currentTrackAtom.reportWrite(value, super.currentTrack, () {
      super.currentTrack = value;
    });
  }

  final _$getPlaylistAsyncAction = AsyncAction('_PlaylistStore.getPlaylist');

  @override
  Future<bool> getPlaylist(String objectId, DateTime date) {
    return _$getPlaylistAsyncAction
        .run(() => super.getPlaylist(objectId, date));
  }

  final _$_setCurrentTrackAsyncAction =
      AsyncAction('_PlaylistStore._setCurrentTrack');

  @override
  Future<void> _setCurrentTrack(DateTime date) {
    return _$_setCurrentTrackAsyncAction
        .run(() => super._setCurrentTrack(date));
  }

  final _$setCurrentTrackAsyncAction =
      AsyncAction('_PlaylistStore.setCurrentTrack');

  @override
  Future<void> setCurrentTrack(Track currentTrack) {
    return _$setCurrentTrackAsyncAction
        .run(() => super.setCurrentTrack(currentTrack));
  }

  final _$playTrackFromAsyncAction =
      AsyncAction('_PlaylistStore.playTrackFrom');

  @override
  Future<void> playTrackFrom(String trackId, String trackType, Duration from) {
    return _$playTrackFromAsyncAction
        .run(() => super.playTrackFrom(trackId, trackType, from));
  }

  final _$_PlaylistStoreActionController =
      ActionController(name: '_PlaylistStore');

  @override
  void setManual(bool manual) {
    final _$actionInfo = _$_PlaylistStoreActionController.startAction(
        name: '_PlaylistStore.setManual');
    try {
      return super.setManual(manual);
    } finally {
      _$_PlaylistStoreActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
playlist: ${playlist},
currentTrack: ${currentTrack},
musicInPlaylist: ${musicInPlaylist},
advertInPlaylist: ${advertInPlaylist}
    ''';
  }
}
