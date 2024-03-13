import 'dart:async';

import 'package:chopper/chopper.dart';
import 'package:flutter/cupertino.dart';
import 'package:mobx/mobx.dart';
import 'package:player_mobile_app/di.dart';
import 'package:player_mobile_app/helpers/format_extensions.dart';
import 'package:player_mobile_app/models/selection.dart';
import 'package:player_mobile_app/services/http/api.dart';
import 'package:player_mobile_app/services/http/models/selection_request.dart';
import 'package:player_mobile_app/stores/object_store.dart';

part 'selection_store.g.dart';

class SelectionStore = _SelectionStore with _$SelectionStore;

abstract class _SelectionStore with Store {
  SelectionsApi _selectionsApi = inject();
  ObjectStore _objectStore = inject();

  @observable
  ObservableList<Selection> selections = ObservableList<Selection>();

  @observable
  Selection selectedSelection;

  @action
  void setSelectedSelection(Selection selection) {
    selectedSelection = selection;
  }

  Future<bool> createSelectionFromSelected(
      DateTime dateBegin, DateTime dateEnd) async {
    var date = DateTime.now().toDate();

    SelectionRequest selectionRequest = SelectionRequest();
    selectionRequest.id = selectedSelection.id;
    selectionRequest.dateBegin = dateBegin;
    selectionRequest.dateEnd = dateEnd;
    selectionRequest.name =
        'Подборка ${_objectStore.selectedObject.name} из ${selectedSelection.name} $date';
    selectionRequest.isPublic = false;
    selectionRequest.tracks = selectedSelection.tracks
        .where((t) => t.selected)
        .map((t) => t.id)
        .toList();

    final response = await _selectionsApi.createSelection(selectionRequest);

    if (response.isSuccessful) {
      //TODO Добавить
      return await getSelections();
    }

    return false;
  }

  @action
  Future<bool> getSelections() async {
    selections.clear();
    final response = await _selectionsApi.getSelections();
    var requests = List<Future<Response<Selection>>>();

    if (response.isSuccessful) {
      for (var selection in response.body.result) {
        requests.add(_selectionsApi.getSelectionById(selection.id));
      }
      var responses = await Future.wait(requests);

      for (var response in responses) {
        if (response.isSuccessful) {
          response.body.tracks.forEach((t) {
            t.selected = true;
          });
          selections.add(response.body);
        }
      }
      return true;
    }

    return false;
  }
}
