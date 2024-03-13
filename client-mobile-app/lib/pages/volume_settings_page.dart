import 'package:flutter/material.dart';
import 'package:flutter_mobx/flutter_mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/helpers/format_extensions.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/selection_store.dart';
import 'package:player_mobile_app/stores/system_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';
import 'package:player_mobile_app/widgets/app_header.dart';
import 'package:sizer/sizer.dart';
import 'package:url_launcher/url_launcher.dart';

class VolumeSettingsPage extends StatefulWidget {
  @override
  _VolumeSettingsPageState createState() => _VolumeSettingsPageState();
}

class _VolumeSettingsPageState extends State<VolumeSettingsPage> {
  final ObjectStore _objectStore = inject();
  final UserStore _userStore = inject();
  final SystemStore _systemStore = inject();
  final PlaylistStore _playlistStore = inject();
  final SelectionStore _selectionStore = inject();

  String _page = 'Music';
  DateTime _start;
  DateTime _end;
  bool _selectionIsCreating = false;
  int _volume;

  var _selectionScroll = ScrollController(
    initialScrollOffset: 0.0,
    keepScrollOffset: true,
  );

  @override
  void initState() {
    _setDefaultDateRange();
    if (_playlistStore.currentTrack == null) {
      _volume = 0;
    } else {
      if (_playlistStore.currentTrack.type == 'Advert') {
        _volume = _objectStore.advertVolume;
      } else {
        _volume = _objectStore.musicVolume;
      }
    }
    super.initState();
  }

  void _setDefaultDateRange() {
    final now = DateTime.now();
    _start = DateTime(now.year, now.month, now.day + 1);
    _end = DateTime(now.year, now.month + 1, now.day + 1);
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
              Expanded(
                child: Card(
                  elevation: 0,
                  shape: RoundedRectangleBorder(
                    side: BorderSide(color: Colors.black54, width: 1),
                    borderRadius: BorderRadius.circular(10),
                  ),
                  child: Column(
                    children: [
                      _page == 'Music' ? _getMusicPage() : _getAdvertPage(),
                      Container(
                        height: 8.0.h,
                        child: Row(
                          mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                          children: [
                            Expanded(
                              child: TextButton(
                                child: Text(
                                  'Музыка',
                                  style: _page == 'Music'
                                      ? TextStyle(color: Colors.white)
                                      : null,
                                ),
                                style: _page == 'Music'
                                    ? TextButton.styleFrom(
                                        backgroundColor: Colors.blue,
                                      )
                                    : null,
                                onPressed: () {
                                  setState(() {
                                    _page = 'Music';
                                  });
                                },
                              ),
                            ),
                            Expanded(
                              child: TextButton(
                                child: Text('Реклама',
                                    style: _page == 'Advert'
                                        ? TextStyle(color: Colors.white)
                                        : null),
                                style: _page == 'Advert'
                                    ? TextButton.styleFrom(
                                        backgroundColor: Colors.blue,
                                      )
                                    : null,
                                onPressed: () {
                                  setState(() {
                                    _page = 'Advert';
                                  });
                                },
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  Widget _getAdvertPage() {
    return Expanded(
      child: Column(
        children: [
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              side: BorderSide(color: Colors.black54, width: 1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Padding(
              padding: const EdgeInsets.only(top: 10),
              child: Column(
                children: [
                  Text('Громкость радио эфира'),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                    children: [
                      Center(
                        child: GestureDetector(
                          child: Image(
                            image: AssetImage('assets/images/left-arrow.png'),
                            width: 10.0.w,
                          ),
                          onTap: () {
                            if (_playlistStore.currentTrack == null) {
                              return;
                            }

                            if (_volume == 0) {
                              return;
                            }
                            setState(() {
                              _volume -= 1;
                            });

                            if (_playlistStore.currentTrack.type == 'Advert') {
                              _objectStore.advertVolume = _volume;
                            } else {
                              _objectStore.musicVolume = _volume;
                            }

                            _objectStore.updateObjectVolume();
                          },
                        ),
                      ),
                      Container(
                        width: 50.0.w,
                        child: Card(
                          shape: RoundedRectangleBorder(
                            side: BorderSide(color: Colors.black54, width: 1),
                            borderRadius: BorderRadius.circular(10),
                          ),
                          child: Padding(
                            padding: const EdgeInsets.all(5.0),
                            child: Column(
                              children: [
                                Row(
                                  mainAxisAlignment: MainAxisAlignment.center,
                                  children: [
                                    Container(
                                      width: 20.0.w,
                                      child: Text(
                                        '${_objectStore.selectedObject.shortTime(_objectStore.selectedObject.beginTime)}',
                                        textAlign: TextAlign.center,
                                      ),
                                    ),
                                    SizedBox(width: 5.0.w),
                                    Container(
                                      width: 20.0.w,
                                      child: Text(
                                        '${_objectStore.selectedObject.shortTime(_objectStore.selectedObject.endTime)}',
                                        textAlign: TextAlign.center,
                                      ),
                                    ),
                                  ],
                                ),
                                SizedBox(height: 1.0.h),
                                Container(
                                  width: 50.0.w,
                                  decoration: BoxDecoration(
                                    color: Colors.white,
                                    border: Border(
                                      top: BorderSide(
                                        width: 1.0,
                                      ),
                                    ),
                                  ),
                                  child: Text(
                                    '$_volume%',
                                    textAlign: TextAlign.center,
                                    style: TextStyle(fontSize: 14.0.sp),
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                      Center(
                        child: GestureDetector(
                          child: Image(
                            image: AssetImage('assets/images/right-arrow.png'),
                            width: 10.0.w,
                          ),
                          onTap: () {
                            if (_playlistStore.currentTrack == null) {
                              return;
                            }

                            if (_volume == 100) {
                              return;
                            }
                            setState(() {
                              _volume += 1;
                            });

                            if (_playlistStore.currentTrack.type == 'Advert') {
                              _objectStore.advertVolume = _volume;
                            } else {
                              _objectStore.musicVolume = _volume;
                            }

                            _objectStore.updateObjectVolume();
                          },
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ),
          Container(
            height: 7.0.h,
            child: Card(
              shape: RoundedRectangleBorder(
                side: BorderSide(color: Colors.black54, width: 1),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Padding(
                // padding: const EdgeInsets.fromLTRB(10, 5, 10, 5),
                padding: const EdgeInsets.all(0),
                child: TextButton(
                  child: Text(
                    'Добавить рекламу',
                  ),
                  onPressed: () async {
                    const url = 'https://909.kz';
                    if (await canLaunch(url)) {
                      await launch(url);
                    }
                  },
                ),
              ),
            ),
          ),
          SizedBox(height: 1.5.h),
          Flexible(
            child: Observer(
              builder: (c) {
                return ListView(
                  children: _getTracks(),
                );
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _getMusicPage() {
    return Expanded(
      child: Column(
        children: [
          _getSelectionHeaders(),
          Observer(builder: (_) => _getTracksInSelectedSelection()),
          Observer(builder: (_) => _getSelectionBottom()),
        ],
      ),
    );
  }

  Widget _getSelectionBottom() {
    if (_selectionStore.selectedSelection == null) {
      return SizedBox(height: 0, width: 0);
    }

    final now = DateTime.now();

    return Container(
      height: 6.0.h,
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceEvenly,
        children: [
          TextButton(
            child: Text('с ${_start.toDate()}'),
            onPressed: () async {
              final result = await showDatePicker(
                  context: context,
                  initialDate: _start,
                  firstDate: DateTime(now.year, now.month, now.day + 1),
                  lastDate: _start.add(const Duration(days: 365)));

              if (result != null && result != _start) {
                setState(() {
                  _start = result;
                });
              }
            },
          ),
          _selectionIsCreating
              ? CircularProgressIndicator()
              : TextButton(
                  child: Text('Старт'),
                  onPressed: !_start.isBefore(_end)
                      ? null
                      : () async {
                          setState(() {
                            _selectionIsCreating = true;
                          });
                          final result = await _selectionStore
                              .createSelectionFromSelected(_start, _end);

                          if (result) {
                            _setDefaultDateRange();
                          }

                          setState(() {
                            _selectionIsCreating = false;
                          });
                        },
                ),
          TextButton(
            child: Text('по ${_end.toDate()}'),
            onPressed: () async {
              final result = await showDatePicker(
                  context: context,
                  initialDate: _end,
                  firstDate: DateTime(now.year, now.month, now.day + 1)
                      .add(const Duration(days: 2)),
                  lastDate: _start.add(const Duration(days: 365)));

              if (result != null && result != _end) {
                setState(() {
                  _end = result;
                });
              }
            },
          ),
        ],
      ),
    );
  }

  Widget _getSelectionHeaders() {
    var items = List<Widget>();

    for (var selection in _selectionStore.selections) {
      items.add(
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 2),
          child: TextButton(
            onPressed: () {
              _selectionStore.setSelectedSelection(selection);
            },
            child: Text(selection.name),
          ),
        ),
      );
    }

    items.add(
      Padding(
        padding: const EdgeInsets.symmetric(horizontal: 2),
        child: IconButton(
          icon: Icon(Icons.add),
          onPressed: () async {
            const url = 'https://909.kz';
            if (await canLaunch(url)) {
              await launch(url);
            }
          },
        ),
      ),
    );

    return Row(
      children: [
        GestureDetector(
          child: Icon(Icons.arrow_left, size: 40),
          onTap: () {
            _selectionScroll.jumpTo(_selectionScroll.offset - 50);
          },
        ),
        Expanded(
          child: SingleChildScrollView(
            scrollDirection: Axis.horizontal,
            controller: _selectionScroll,
            child: Row(children: items),
          ),
        ),
        GestureDetector(
          child: Icon(Icons.arrow_right, size: 40),
          onTap: () {
            _selectionScroll.jumpTo(_selectionScroll.offset + 50);
          },
        ),
      ],
    );
  }

  Widget _getTracksInSelectedSelection() {
    if (_selectionStore.selectedSelection == null) {
      return SizedBox(height: 0, width: 0);
    }

    var items = List<Widget>();
    final height = 6.0.h;

    for (var track in _selectionStore.selectedSelection.tracks) {
      final item = Container(
        height: height,
        decoration: BoxDecoration(
          border: Border(
            bottom: BorderSide(
              color: Colors.black54,
            ),
          ),
        ),
        child: Row(
          mainAxisAlignment: MainAxisAlignment.start,
          children: [
            GestureDetector(
              onTap: () {
                _playlistStore.setManual(true);
                _playlistStore.playTrackFrom(track.id, 'Music', Duration.zero);
              },
              child: Container(
                child: Center(
                  child: Icon(Icons.play_circle_outline, size: 30),
                ),
                width: 40,
                height: height,
                decoration: BoxDecoration(
                  border: Border(
                    right: BorderSide(
                      color: Colors.black54,
                    ),
                  ),
                ),
              ),
            ),
            Container(
              width: 230,
              height: height,
              padding: const EdgeInsets.symmetric(horizontal: 2),
              child: Center(
                child: Text(
                  track.name,
                  overflow: TextOverflow.ellipsis,
                  style: TextStyle(fontSize: 14),
                  textAlign: TextAlign.center,
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
              width: 40,
              height: height,
              child: Center(
                child: Text(
                  Duration(seconds: track.length.round()).toMinuteSeconds(),
                  style: TextStyle(fontSize: 14),
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
              width: 40,
              height: height,
              child: Center(
                child: Checkbox(
                  value: track.selected,
                  onChanged: (value) {
                    setState(() {
                      track.selected = value;
                    });
                  },
                ),
              ),
            ),
          ],
        ),
      );
      items.add(item);
    }

    return Flexible(child: ListView(children: items));
  }

  List<Widget> _getTracks() {
    var tracks = _objectStore.advertsOnObject;
    var widgets = List<Widget>();

    final height = 6.0.h;
    for (var track in tracks) {
      widgets.add(
        Container(
          height: height,
          decoration: BoxDecoration(
            border: Border(
              bottom: BorderSide(
                color: Colors.black54,
              ),
            ),
          ),
          child: Row(
            mainAxisAlignment: MainAxisAlignment.start,
            children: [
              GestureDetector(
                onTap: () {
                  _playlistStore.setManual(true);
                  _playlistStore.playTrackFrom(
                      track.id, 'Advert', Duration.zero);
                },
                child: Container(
                  child: Center(
                    child: Icon(Icons.play_circle_outline, size: 30),
                  ),
                  width: 11.0.w,
                  height: height,
                  decoration: BoxDecoration(
                    border: Border(
                      right: BorderSide(
                        color: Colors.black54,
                      ),
                    ),
                  ),
                ),
              ),
              Container(
                child: Center(
                  child: Text(
                    track.name,
                    overflow: TextOverflow.ellipsis,
                    textAlign: TextAlign.center,
                  ),
                ),
                width: 43.0.w,
                height: height,
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
                    Duration(seconds: track.length.round()).toMinuteSeconds(),
                  ),
                ),
                width: 11.0.w,
                height: height,
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
                  child: Text('с ${track.fromDate.toDayMonth()}'),
                ),
                width: 15.0.w,
                height: height,
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
                  child: Text('по ${track.toDate.toDayMonth()}'),
                ),
                width: 16.0.w,
                height: height,
              ),
            ],
          ),
        ),
      );
    }

    return widgets;
  }
}
