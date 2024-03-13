import 'package:sizer/sizer.dart';
import 'package:fluro/fluro.dart';
import 'package:flutter/material.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/routes/routes.dart';
import 'package:player_mobile_app/stores/object_store.dart';
import 'package:player_mobile_app/stores/playlist_store.dart';
import 'package:player_mobile_app/stores/selection_store.dart';
import 'package:player_mobile_app/stores/user_store.dart';

class LoginPage extends StatefulWidget {
  @override
  _LoginPageState createState() => _LoginPageState();
}

class _LoginPageState extends State<LoginPage> {
  String _email = '';
  String _password = '';
  bool _isLoading = false;

  final UserStore _userStore = inject();
  final ObjectStore _objectStore = inject();
  final PlaylistStore _playlistStore = inject();
  final SelectionStore _selectionStore = inject();
  final _router = inject<FluroRouter>();

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: ListView(
        children: [
          Column(
            mainAxisAlignment: MainAxisAlignment.start,
            crossAxisAlignment: CrossAxisAlignment.center,
            children: [
              SizedBox(height: 5.0.h),
              Text('Мобильное приложение'),
              SizedBox(height: 1.0.h),
              Image(
                image: AssetImage('assets/images/logo.png'),
                width: 80.0.w,
              ),
              SizedBox(height: 1.0.h),
              Text('Радиовещние в Торговых центрах РК'),
              SizedBox(height: 2.0.h),
              Stack(
                children: [
                  Center(
                    child: Container(
                      height: 5.0.h,
                      width: 50.0.w,
                      decoration: BoxDecoration(
                        color: Colors.white,
                        border: Border.all(width: 1.0),
                        borderRadius: BorderRadius.all(Radius.circular(5.0)),
                      ),
                      child: Padding(
                        padding: const EdgeInsets.all(1.0),
                        child: Center(
                          child: Text(
                            'Вход',
                            style: TextStyle(
                              fontSize: 15.0.sp,
                            ),
                          ),
                        ),
                      ),
                      transform: Matrix4.translationValues(0, 7, 0),
                    ),
                  ),
                ],
              ),
              _buildLoginCard(),
              SizedBox(height: 20.0.h),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildLoginCard() {
    return Center(
      child: Column(
        children: [
          Card(
            elevation: 0,
            shape: RoundedRectangleBorder(
              side: BorderSide(color: Colors.black54, width: 1),
              borderRadius: BorderRadius.circular(10),
            ),
            child: Padding(
              padding: EdgeInsets.only(top: 2.0.h),
              child: Container(
                padding: EdgeInsets.symmetric(horizontal: 5.0.w),
                child: Column(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: TextField(
                            keyboardType: TextInputType.emailAddress,
                            decoration: InputDecoration(
                              contentPadding:
                                  EdgeInsets.fromLTRB(20.0, 15.0, 20.0, 15.0),
                              hintText: 'Логин',
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(32.0),
                              ),
                            ),
                            onChanged: (value) {
                              _email = value;
                            },
                          ),
                        ),
                      ],
                    ),
                    SizedBox(height: 1.0.h),
                    Row(
                      children: [
                        Expanded(
                          child: TextField(
                            obscureText: true,
                            decoration: InputDecoration(
                              contentPadding:
                                  EdgeInsets.fromLTRB(20.0, 15.0, 20.0, 15.0),
                              hintText: 'Пароль',
                              border: OutlineInputBorder(
                                borderRadius: BorderRadius.circular(32.0),
                              ),
                            ),
                            onChanged: (value) {
                              _password = value;
                            },
                          ),
                        ),
                      ],
                    ),
                    SizedBox(height: 1.0.h),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        _isLoading
                            ? CircularProgressIndicator()
                            : Expanded(
                                child: ElevatedButton(
                                  onPressed: _loginPressed,
                                  child: Text('Войти'),
                                ),
                              ),
                      ],
                    ),
                    SizedBox(height: 1.0.h),
                  ],
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  Future _loginPressed() async {
    setState(() {
      _isLoading = true;
    });
    try {
      final success = await _userStore.login(_email, _password);
      if (!success) {
        setState(() {
          _isLoading = false;
        });
        return;
      }
      if (await _userStore.getPermissions()) {
        await _objectStore.setUserObject();
        await _playlistStore.getPlaylist(
            _objectStore.selectedObject.id, DateTime.now());
        await _selectionStore.getSelections();

        setState(() {
          _isLoading = false;
        });
        await _router.navigateTo(context, Routes.main, clearStack: false);
      }
    } on Exception {
      setState(() {
        _isLoading = false;
      });
    }
  }
}
