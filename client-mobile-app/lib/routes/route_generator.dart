import 'package:fluro/fluro.dart';
import 'package:flutter/material.dart';
import 'package:flutter/widgets.dart';
import 'package:player_mobile_app/pages/login_page.dart';
import 'package:player_mobile_app/pages/main_page.dart';
import 'package:player_mobile_app/pages/select_object_page.dart';
import 'package:player_mobile_app/pages/volume_settings_page.dart';

import 'routes.dart';

class RouterHelper {
  static void setupRoutes(FluroRouter router) {
    router.define(
      Routes.main,
      handler: Handler(
          handlerFunc: (BuildContext context, Map<String, dynamic> params) {
        return MainPage();
      }),
      transitionType: TransitionType.material,
    );
    router.define(
      Routes.login,
      handler: Handler(
          handlerFunc: (BuildContext context, Map<String, dynamic> params) {
        return LoginPage();
      }),
      transitionType: TransitionType.material,
    );
    router.define(
      Routes.volumeSettings,
      handler: Handler(
          handlerFunc: (BuildContext context, Map<String, dynamic> params) {
        return VolumeSettingsPage();
      }),
      transitionType: TransitionType.material,
    );
    router.define(
      Routes.selectObject,
      handler: Handler(
          handlerFunc: (BuildContext context, Map<String, dynamic> params) {
        return SelectObjectPage();
      }),
      transitionType: TransitionType.material,
    );
  }
}
