import 'package:fluro/fluro.dart';
import 'package:flutter/material.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/routes/routes.dart';
import 'package:player_mobile_app/services/online_connector.dart';
import 'package:player_mobile_app/services/store/user_settings_repository.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/selection_store.dart';
import 'package:player_mobile_app/stores/system_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';
import 'package:player_mobile_app/widgets/app_header.dart';
import 'package:sizer/sizer.dart';

class SelectObjectPage extends StatefulWidget {
  @override
  _SelectObjectPageState createState() => _SelectObjectPageState();
}

class _SelectObjectPageState extends State<SelectObjectPage> {
  final ObjectStore _objectStore = inject();
  final UserStore _userStore = inject();
  final SystemStore _systemStore = inject();
  final PlaylistStore _playlistStore = inject();
  final SelectionStore _selectionStore = inject();
  final UserSettingsRepository _userSettingsRepository = inject();
  final OnlineConnector _onlineConnector = inject();
  final FluroRouter _router = inject();

  bool _isLoading = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Stack(
        children: [
          SafeArea(
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
                        children: _getObjects(),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
          if (_isLoading)
            Container(
              color: Colors.black.withOpacity(0.5),
              child: Center(
                child: CircularProgressIndicator(),
              ),
            ),
        ],
      ),
    );
  }

  List<Widget> _getObjects() {
    final objects = _objectStore.objects;
    var widgets = List<Widget>();

    final height = 6.0.h;
    for (var object in objects) {
      widgets.add(
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
          child: GestureDetector(
            onTap: () async {
              setState(() {
                _isLoading = true;
              });

              await _objectStore.getObjectInfo(object.id);
              await _objectStore.getAdvertsOnObject(object.id);
              await _playlistStore.getPlaylist(
                  _objectStore.selectedObject.id, DateTime.now());
              _onlineConnector.setSelectedObjectId(object.id);

              await _userSettingsRepository.saveLastSelectedObject(object.id);

              setState(() {
                _isLoading = false;
              });

              _router.navigateTo(context, Routes.main, clearStack: true);
            },
            child: Container(
              child: Center(
                child: Text(
                  object.name,
                  overflow: TextOverflow.ellipsis,
                  textAlign: TextAlign.center,
                ),
              ),
              height: height,
              decoration: BoxDecoration(
                border: Border.all(width: 1.0),
                borderRadius: BorderRadius.all(Radius.circular(5.0)),
              ),
            ),
          ),
        ),
      );
    }

    return widgets;
  }
}
