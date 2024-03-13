import 'package:fluro/fluro.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/helpers/format_extensions.dart';
import 'package:player_mobile_app/routes/routes.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/system_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';
import 'package:player_mobile_app/widgets/seek_bar.dart';
import 'package:sizer/sizer.dart';

class AppHeader extends StatelessWidget {
  final ObjectStore _objectStore = inject();
  final UserStore _userStore = inject();
  final SystemStore _systemStore = inject();
  final PlaylistStore _playlistStore = inject();
  final FluroRouter _router = inject();

  @override
  Widget build(BuildContext context) {
    return Stack(
      children: [
        Card(
          shape: RoundedRectangleBorder(
            side: BorderSide(color: Colors.black54, width: 1),
            borderRadius: BorderRadius.circular(10),
          ),
          elevation: 0,
          child: Padding(
            padding: const EdgeInsets.only(top: 15),
            child: Column(
              children: [
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                  children: [
                    Column(
                      children: [
                        Observer(
                          builder: (_) => Text(
                            _objectStore.selectedObject.city.name,
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                        Observer(
                          builder: (_) => Text(
                            _systemStore.currentDate.toDate(),
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                        Observer(
                          builder: (_) => Text(
                            _systemStore.currentDate.toFullTime(),
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                      ],
                    ),
                    SizedBox(width: 10),
                    Column(
                      children: [
                        Observer(
                          builder: (_) => GestureDetector(
                            onTap: () {
                              _router.navigateTo(context, Routes.selectObject);
                            },
                            child: ConstrainedBox(
                              constraints: BoxConstraints(maxWidth: 50.0.w),
                              child: Container(
                                decoration: BoxDecoration(
                                  color: Colors.white,
                                  border: Border.all(width: 1.0),
                                  borderRadius:
                                      BorderRadius.all(Radius.circular(5.0)),
                                ),
                                child: Text(
                                  _objectStore.selectedObject.name,
                                  style: TextStyle(fontSize: 12.0.sp),
                                  overflow: TextOverflow.ellipsis,
                                ),
                              ),
                            ),
                          ),
                        ),
                        Observer(
                          builder: (_) => Text(
                            'Рекламы: ${_playlistStore.advertInPlaylist}',
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                        Observer(
                          builder: (_) => Text(
                            'Музыки: ${_playlistStore.musicInPlaylist}',
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                      ],
                    ),
                    SizedBox(width: 10),
                    Column(
                      children: [
                        Observer(
                          builder: (_) => _objectStore.selectedObject.isOnline
                              ? Icon(
                                  Icons.wifi,
                                  color: Colors.green,
                                )
                              : Icon(Icons.wifi_off, color: Colors.red),
                        ),
                        Observer(
                          builder: (_) => Text(
                            _objectStore.selectedObject.beginTime,
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                        Observer(
                          builder: (_) => Text(
                            _objectStore.selectedObject.endTime,
                            style: TextStyle(fontSize: 12.0.sp),
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
                SizedBox(height: 3.0.h),
                //TODO Поднять текст и вставить плеер
                Center(
                  child: Text(
                    'Сейчас играет плейлист за ${_playlistStore.playlist.date.toDate()}',
                    style: TextStyle(fontSize: 11.0.sp),
                  ),
                ),
                Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Expanded(
                      child: StreamBuilder<Duration>(
                        stream: _playlistStore.player.positionStream,
                        builder: (context, snapshot) {
                          return SeekBar(
                            duration:
                                _playlistStore.player.duration ?? Duration.zero,
                            bufferedPosition: Duration.zero,
                            position: snapshot.data ?? Duration.zero,
                            onChangeEnd: (newPosition) {
                              _playlistStore.player.seek(newPosition);
                            },
                          );
                        },
                      ),
                    ),
                    IconButton(
                        icon: Icon(Icons.settings),
                        onPressed: () {
                          if (ModalRoute.of(context).settings.name ==
                              Routes.main) {
                            _router.navigateTo(context, Routes.volumeSettings);
                            return;
                          }
                          if (ModalRoute.of(context).settings.name ==
                              Routes.volumeSettings) {
                            _router.navigateTo(context, Routes.main);
                            return;
                          }
                        }),
                    IconButton(
                        icon: Icon(Icons.logout),
                        onPressed: () {
                          _router.navigateTo(context, Routes.login);
                        }),
                  ],
                ),
              ],
            ),
          ),
        ),
        Center(
          child: Container(
            decoration: BoxDecoration(
              color: Colors.white,
              border: Border.all(width: 1.0),
              borderRadius: BorderRadius.all(Radius.circular(5.0)),
            ),
            child: Padding(
              padding: const EdgeInsets.all(1.0),
              child: Observer(
                  builder: (_) => Text('Партнер ${_userStore.userEmail}')),
            ),
            transform: Matrix4.translationValues(0, -3, 0),
          ),
        ),
      ],
    );
  }
}
