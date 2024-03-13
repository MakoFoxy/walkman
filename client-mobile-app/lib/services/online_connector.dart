import 'dart:async';
import 'dart:convert';

import 'package:package_info/package_info.dart';
import 'package:player_mobile_app/helpers/constants.dart';
import 'package:player_mobile_app/models/online_object_info.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:signalr_netcore/signalr_client.dart';

class OnlineConnector {
  static HubConnection _hubConnection;

  static bool get isConnected =>
      _hubConnection != null &&
      _hubConnection.state == HubConnectionState.Connected;

  PlaylistStore _playlistStore;
  String _lastObjectId;

  OnlineConnector(PlaylistStore playlistStore) : _playlistStore = playlistStore;

  Future<void> init() async {
    PackageInfo packageInfo = await PackageInfo.fromPlatform();
    final version = '${packageInfo.version}+${packageInfo.buildNumber}';

    final String url =
        '${Constants.publisherEndpoint}/ws/player-client-hub?&version=$version&isMobile=true';
    _hubConnection = HubConnectionBuilder().withUrl(url).build();

    _hubConnection.onclose(({error}) async {
      await Future.delayed(const Duration(seconds: 5));
      await _hubConnection.start();
    });

    _hubConnection.onreconnected(({connectionId}) {
      _hubConnection.invoke('SetSelectedObjectId', args: [_lastObjectId]);
    });

    _hubConnection.on('CurrentTrackResponse', _currentTrackResponse);

    try {
      await _hubConnection.start();
    } on Exception catch (e) {
      print(e);
    }
  }

  Future<void> setSelectedObjectId(String objectId) async {
    _lastObjectId = objectId;
    if (_hubConnection.state == HubConnectionState.Disconnected) {
      return Future.value(null);
    }

    return _hubConnection.invoke('SetSelectedObjectId', args: [objectId]);
  }

  void _currentTrackResponse(List<Object> parameters) {
    final info = OnlineObjectInfo.fromJson(jsonDecode(parameters.first));

    if (info.currentTrack == null ||
        info.currentTrack.isEmpty ||
        info.objectId != _lastObjectId) {
      return;
    }

    if (_playlistStore.currentTrack == null) {
      _setCurrentTrack(info);
      return;
    }

    if (info.currentTrack == _playlistStore.currentTrack.uniqueId) {
      _playlistStore.player.seek(Duration(seconds: info.secondsFromStart));
      return;
    }

    _setCurrentTrack(info);
  }

  Future<void> _setCurrentTrack(OnlineObjectInfo info) async {
    final track = _playlistStore.playlist.tracks
        .firstWhere((t) => t.uniqueId == info.currentTrack, orElse: () => null);

    if (track == null) {
      return;
    }

    _playlistStore.setManual(true);
    try {
      await _playlistStore.setCurrentTrack(track);
      final position = Duration(seconds: info.secondsFromStart);
      await _playlistStore.playTrackFrom(track.id, track.type, position);
    } on Exception catch (e) {
      print(e);
    }
  }
}
