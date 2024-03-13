// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'object_store.dart';

// **************************************************************************
// StoreGenerator
// **************************************************************************

// ignore_for_file: non_constant_identifier_names, unnecessary_brace_in_string_interps, unnecessary_lambdas, prefer_expression_function_bodies, lines_longer_than_80_chars, avoid_as, avoid_annotating_with_dynamic

mixin _$ObjectStore on _ObjectStore, Store {
  final _$selectedObjectAtom = Atom(name: '_ObjectStore.selectedObject');

  @override
  ObjectInfo get selectedObject {
    _$selectedObjectAtom.reportRead();
    return super.selectedObject;
  }

  @override
  set selectedObject(ObjectInfo value) {
    _$selectedObjectAtom.reportWrite(value, super.selectedObject, () {
      super.selectedObject = value;
    });
  }

  final _$objectsAtom = Atom(name: '_ObjectStore.objects');

  @override
  List<DictionaryItem> get objects {
    _$objectsAtom.reportRead();
    return super.objects;
  }

  @override
  set objects(List<DictionaryItem> value) {
    _$objectsAtom.reportWrite(value, super.objects, () {
      super.objects = value;
    });
  }

  final _$advertsOnObjectAtom = Atom(name: '_ObjectStore.advertsOnObject');

  @override
  List<AdvertModel> get advertsOnObject {
    _$advertsOnObjectAtom.reportRead();
    return super.advertsOnObject;
  }

  @override
  set advertsOnObject(List<AdvertModel> value) {
    _$advertsOnObjectAtom.reportWrite(value, super.advertsOnObject, () {
      super.advertsOnObject = value;
    });
  }

  final _$musicVolumeAtom = Atom(name: '_ObjectStore.musicVolume');

  @override
  int get musicVolume {
    _$musicVolumeAtom.reportRead();
    return super.musicVolume;
  }

  @override
  set musicVolume(int value) {
    _$musicVolumeAtom.reportWrite(value, super.musicVolume, () {
      super.musicVolume = value;
    });
  }

  final _$advertVolumeAtom = Atom(name: '_ObjectStore.advertVolume');

  @override
  int get advertVolume {
    _$advertVolumeAtom.reportRead();
    return super.advertVolume;
  }

  @override
  set advertVolume(int value) {
    _$advertVolumeAtom.reportWrite(value, super.advertVolume, () {
      super.advertVolume = value;
    });
  }

  final _$setUserObjectAsyncAction = AsyncAction('_ObjectStore.setUserObject');

  @override
  Future<bool> setUserObject() {
    return _$setUserObjectAsyncAction.run(() => super.setUserObject());
  }

  final _$getObjectInfoAsyncAction = AsyncAction('_ObjectStore.getObjectInfo');

  @override
  Future<bool> getObjectInfo(String objectId) {
    return _$getObjectInfoAsyncAction.run(() => super.getObjectInfo(objectId));
  }

  final _$getAdvertsOnObjectAsyncAction =
      AsyncAction('_ObjectStore.getAdvertsOnObject');

  @override
  Future<bool> getAdvertsOnObject(String objectId) {
    return _$getAdvertsOnObjectAsyncAction
        .run(() => super.getAdvertsOnObject(objectId));
  }

  final _$updateObjectVolumeAsyncAction =
      AsyncAction('_ObjectStore.updateObjectVolume');

  @override
  Future<bool> updateObjectVolume() {
    return _$updateObjectVolumeAsyncAction
        .run(() => super.updateObjectVolume());
  }

  @override
  String toString() {
    return '''
selectedObject: ${selectedObject},
objects: ${objects},
advertsOnObject: ${advertsOnObject},
musicVolume: ${musicVolume},
advertVolume: ${advertVolume}
    ''';
  }
}
