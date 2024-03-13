import 'package:sizer/sizer.dart';
import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/helpers/format_extensions.dart';
import 'package:player_mobile_app/models/track.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';
import 'package:player_mobile_app/widgets/app_header.dart';

class MainPage extends StatefulWidget {
  @override
  _MainPageState createState() => _MainPageState();
}

class _MainPageState extends State<MainPage> {
  final UserStore _userStore = inject();
  final PlaylistStore _playlistStore = inject();

  var _playlistScrollController = ScrollController(
    initialScrollOffset: 0.0,
    keepScrollOffset: true,
  );

  @override
  void initState() {
    _playlistStore.setManual(false);
    super.initState();
  }

  @override
  void dispose() {
    _playlistScrollController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.only(top: 10),
          child: Column(
            children: [
              AppHeader(),
              SizedBox(height: 3.0.h),
              Expanded(
                child: Observer(
                  builder: (c) {
                    return ListView(
                      controller: _playlistScrollController,
                      children: _trackList(c),
                    );
                  },
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  List<Widget> _trackList(BuildContext context) {
    if (_playlistStore.playlist.tracks == null) {
      return [
        SizedBox(height: 0, width: 0),
      ];
    }
    final trackRowHeight = 6.0.h;
    const halfRowsCount = 5;
    final currentTrackIndex =
        _playlistStore.playlist.tracks.indexOf(_playlistStore.currentTrack);

    Future.delayed(const Duration(milliseconds: 500), () {
      if (_playlistScrollController.hasClients) {
        _playlistScrollController.jumpTo(trackRowHeight * currentTrackIndex -
            halfRowsCount * trackRowHeight);
      }
    });

    var tracks = <Widget>[];
    for (var track in _playlistStore.playlist.tracks) {
      Widget trackWidget = Container(
        height: trackRowHeight,
        decoration: BoxDecoration(
          border: Border(
            bottom: BorderSide(
              color: Colors.black54,
            ),
          ),
          color: _getTrackColor(track),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.start,
          children: [
            Container(
              child: Center(
                child: Text(
                  track.index.toString(),
                  style: TextStyle(fontSize: 12.0.sp),
                ),
              ),
              width: 8.0.w,
              height: 6.0.h,
              decoration: BoxDecoration(
                border: Border(
                  right: BorderSide(
                    color: Colors.black54,
                  ),
                ),
              ),
            ),
            Container(
              child: Center(
                child: Text(
                  track.repeatNumber.toString(),
                  style: TextStyle(fontSize: 12.0.sp),
                ),
              ),
              width: 8.0.w,
              height: 6.0.h,
              decoration: BoxDecoration(
                border: Border(
                  right: BorderSide(
                    color: Colors.black54,
                  ),
                ),
              ),
            ),
            Container(
              child: Center(
                child: Text(
                  track.playingDateTime.toFullTime(),
                  style: TextStyle(fontSize: 12.0.sp),
                ),
              ),
              width: 16.0.w,
              height: 6.0.h,
              decoration: BoxDecoration(
                border: Border(
                  right: BorderSide(
                    color: Colors.black54,
                  ),
                ),
              ),
            ),
            Container(
              width: 55.0.w,
              height: 6.0.h,
              padding: const EdgeInsets.symmetric(horizontal: 2),
              child: Center(
                child: Text(
                  track.nameWithoutExtension(),
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 12.0.sp),
                ),
              ),
              decoration: BoxDecoration(
                border: Border(
                  right: BorderSide(
                    color: Colors.black54,
                  ),
                ),
              ),
            ),
            Container(
              width: 11.0.w,
              height: 6.0.h,
              child: Center(
                child: Text(
                  Duration(seconds: track.length.round()).toMinuteSeconds(),
                  style: TextStyle(fontSize: 12.0.sp),
                ),
              ),
            ),
          ],
        ),
      );

      if (track.type == 'Music' && _userStore.isManager) {
        trackWidget = Dismissible(
          key: Key(track.uniqueId),
          child: trackWidget,
          direction: DismissDirection.startToEnd,
          background: Container(
            color: Color.fromARGB(255, 247, 84, 84),
            child: Padding(
              padding: EdgeInsets.fromLTRB(
                  5, trackRowHeight - trackRowHeight * 0.7, 0, 0),
              child: Text('Удаление из плейлиста'),
            ),
          ),
          onDismissed: (direction) async {
            await _userStore.banMusic(track);
            _playlistStore.playlist.tracks.remove(track);
            //TODO
            // Get.showSnackbar(
            //   GetBar(
            //     message:
            //         'Трек ${track.nameWithoutExtension()} больше не будет играть на вашем объекте',
            //     duration: const Duration(seconds: 2),
            //   ),
            // );
          },
          confirmDismiss: (direction) async {
            return await showDialog(
              context: context,
              builder: (BuildContext context) {
                return AlertDialog(
                  title: const Text("Подтверждение"),
                  content: const Text(
                      "Вы уверены что хотите убрать трек из плейлиста?"),
                  actions: <Widget>[
                    FlatButton(
                      onPressed: () => Navigator.of(context).pop(false),
                      child: const Text("Нет"),
                    ),
                    FlatButton(
                      onPressed: () => Navigator.of(context).pop(true),
                      child: const Text("Да"),
                    ),
                  ],
                );
              },
            );
          },
        );
      }

      tracks.add(trackWidget);
    }

    return tracks;
  }

  Color _getTrackColor(Track track) {
    if (track == _playlistStore.currentTrack) {
      return Color.fromARGB(255, 233, 191, 255);
    }
    return track.type == 'Advert'
        ? Color.fromARGB(255, 191, 255, 212)
        : Color.fromARGB(255, 255, 248, 203);
  }
}
