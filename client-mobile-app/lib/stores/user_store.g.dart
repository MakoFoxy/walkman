// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'user_store.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic

mixin _$UserStore on _UserStore, Store {
  Computed<bool> _$isManagerComputed;

  @override
  bool get isManager => (_$isManagerComputed ??=
          Computed<bool>(() => super.isManager, name: '_UserStore.isManager'))
      .value;

  final _$tokenAtom = Atom(name: '_UserStore.token');

  @override
  String get token {
    _$tokenAtom.reportRead();
    return super.token;
  }

  @override
  set token(String value) {
    _$tokenAtom.reportWrite(value, super.token, () {
      super.token = value;
    });
  }

  final _$userEmailAtom = Atom(name: '_UserStore.userEmail');

  @override
  String get userEmail {
    _$userEmailAtom.reportRead();
    return super.userEmail;
  }

  @override
  set userEmail(String value) {
    _$userEmailAtom.reportWrite(value, super.userEmail, () {
      super.userEmail = value;
    });
  }

  final _$permissionsAtom = Atom(name: '_UserStore.permissions');

  @override
  List<String> get permissions {
    _$permissionsAtom.reportRead();
    return super.permissions;
  }

  @override
  set permissions(List<String> value) {
    _$permissionsAtom.reportWrite(value, super.permissions, () {
      super.permissions = value;
    });
  }

  final _$loginAsyncAction = AsyncAction('_UserStore.login');

  @override
  Future<bool> login(String email, String password) {
    return _$loginAsyncAction.run(() => super.login(email, password));
  }

  final _$getPermissionsAsyncAction = AsyncAction('_UserStore.getPermissions');

  @override
  Future<bool> getPermissions() {
    return _$getPermissionsAsyncAction.run(() => super.getPermissions());
  }

  final _$banMusicAsyncAction = AsyncAction('_UserStore.banMusic');

  @override
  Future<bool> banMusic(Track musicTrack) {
    return _$banMusicAsyncAction.run(() => super.banMusic(musicTrack));
  }

  final _$_UserStoreActionController = ActionController(name: '_UserStore');

  @override
  void setUserAuthData(String email, String token) {
    final _$actionInfo = _$_UserStoreActionController.startAction(
        name: '_UserStore.setUserAuthData');
    try {
      return super.setUserAuthData(email, token);
    } finally {
      _$_UserStoreActionController.endAction(_$actionInfo);
    }
  }

  @override
  String toString() {
    return '''
token: ${token},
userEmail: ${userEmail},
permissions: ${permissions},
isManager: ${isManager}
    ''';
  }
}
