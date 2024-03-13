import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:get_it/get_it.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:player_mobile_app/app.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/routes/route_generator.dart';
import 'package:player_mobile_app/routes/routes.dart';
import 'package:player_mobile_app/services/first_screen_qualifier.dart';
import 'package:player_mobile_app/services/online_connector.dart';
import 'package:player_mobile_app/services/store/user_settings_repository.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/selection_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();
  await initializeDateFormatting();
  final ioc = GetIt.instance;
  await setupDependency();
  RouterHelper.setupRoutes(ioc.get());
  await SystemChrome.setPreferredOrientations([DeviceOrientation.portraitUp]);
  await _setFirstScreen(ioc);

  runApp(App());
}

Future _setFirstScreen(GetIt ioc) async {
  final firstScreenQualifier = ioc.get<FirstScreenQualifier>();
  Routes.firstScreen = await firstScreenQualifier.getFirstScreen();

  if (Routes.firstScreen != Routes.login) {
    final UserStore _userStore = inject();
    final ObjectStore _objectStore = inject();
    final PlaylistStore _playlistStore = inject();
    final SelectionStore _selectionStore = inject();
    final UserSettingsRepository _userSettingsRepository = inject();
    final OnlineConnector _onlineConnector = inject();

    _userStore.setUserAuthData(
      _userSettingsRepository.getUserEmail(),
      _userSettingsRepository.getUserToken(),
    );

    if (await _userStore.getPermissions()) {
      await _objectStore.setUserObject();
      await _playlistStore.getPlaylist(
          _objectStore.selectedObject.id, DateTime.now());
      _onlineConnector.setSelectedObjectId(_objectStore.selectedObject.id);
      await _selectionStore.getSelections();
    } else {
      Routes.firstScreen = Routes.login;
    }
  }
}
