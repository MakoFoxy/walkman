import 'package:fluro/fluro.dart';
import 'package:flutter/material.dart';
import 'package:player_mobile_app/routes/routes.dart';
import 'package:sizer/sizer.dart';

class App extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    return LayoutBuilder(builder: (context, constraints) {
      return OrientationBuilder(builder: (context, orientation) {
        SizerUtil().init(constraints, orientation);

        return MaterialApp(
          title: 'Плеер объекта',
          debugShowCheckedModeBanner: false,
          initialRoute: Routes.firstScreen,
          onGenerateRoute: FluroRouter.appRouter.generator,
          theme: ThemeData(
            primarySwatch: Colors.blue,
            visualDensity: VisualDensity.adaptivePlatformDensity,
          ),
        );
      });
    });
  }
}
