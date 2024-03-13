import 'dart:async';

import 'package:just_audio/just_audio.dart';
import 'package:mobx/mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/helpers/constants.dart';
import 'package:player_mobile_app/models/playlist.dart';
import 'package:player_mobile_app/models/track.dart';
import 'package:player_mobile_app/services/http/api.dart';
import 'package:player_mobile_app/services/online_connector.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/system_store.dart';

part 'playlist_store.g.dart';

class PlaylistStore = _PlaylistStore with _$PlaylistStore;

abstract class _PlaylistStore with Store {
  PlaylistApi _playerApi = inject();
  SystemStore _systemStore = inject();
  ObjectStore _objectStore = inject();

  @observable
  Playlist playlist = Playlist();

  @observable
  Track currentTrack = Track();

  AudioPlayer player = AudioPlayer();

  bool _manualPlay = false;

  @computed
  int get musicInPlaylist => playlist.tracks
      .where((t) => t.type == 'Music')
      .map((t) => t.id)
      .toSet()
      .length;

  @computed
  int get advertInPlaylist => playlist.tracks
      .where((t) => t.type == 'Advert')
      .map((t) => t.id)
      .toSet()
      .length;

  @action
  Future<bool> getPlaylist(String objectId, DateTime date) async {
    final response = await _playerApi.getPlaylist(objectId, date);

    if (response.isSuccessful) {
      response.body.playlist.tracks
          .sort((a, b) => a.playingDateTime.compareTo(b.playingDateTime));

      int index = 1;

      for (var track in response.body.playlist.tracks) {
        track.index = index;

        track.repeatNumber = response.body.playlist.tracks
                .where((t) =>
                    t.id == track.id &&
                    t.playingDateTime.isBefore(track.playingDateTime))
                .length +
            1;

        index++;
      }

      playlist = response.body.playlist;
      return true;
    }

    return false;
  }

  void init() {
    _systemStore.subscribe((date) => _setCurrentTrack(date));
  }

  @action
  Future<void> _setCurrentTrack(DateTime date) async {
    //TODO Проверить почему _objectStore.selectedObject.isOnline при старте null
    if (_manualPlay ||
        (OnlineConnector.isConnected &&
            _objectStore.selectedObject.isOnline != null &&
            _objectStore.selectedObject.isOnline)) {
      return;
    }

    if (playlist?.tracks != null && playlist.tracks.isNotEmpty) {
      final currentTime = DateTime.now();
      var currentTrack = playlist.tracks.firstWhere(
          (t) =>
              t.playingDateTime.isBefore(currentTime) &&
              t.playingDateTime
                  .add(Duration(seconds: t.length.round()))
                  .isAfter(currentTime),
          orElse: () => null);

      if (currentTrack == null && player.playing) {
        player.stop();
        player.seek(const Duration(seconds: 0));
      }

      if (currentTrack == null) {
        return;
      }

      if (this.currentTrack != currentTrack) {
        await setCurrentTrack(currentTrack);
        await playTrackFrom(currentTrack.id, currentTrack.type, Duration.zero);
      }
    }
  }

  @action
  Future<void> setCurrentTrack(Track currentTrack) async {
    this.currentTrack = currentTrack;
  }

  @action
  void setManual(bool manual) {
    _manualPlay = manual;
  }

  @action
  Future<void> playTrackFrom(
    String trackId,
    String trackType,
    Duration from,
  ) async {
    try {
      await player.setUrl(
          '${Constants.publisherEndpoint}/api/v1/track?trackId=$trackId&TrackType=$trackType');

      if (player.playing) {
        await player.stop();
      }
      await player.load();
      await player.seek(from);
      await player.play();
    } on Exception catch (e) {
      print(e.toString());
    }
  }
}
