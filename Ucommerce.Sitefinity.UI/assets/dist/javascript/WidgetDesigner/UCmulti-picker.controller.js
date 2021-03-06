﻿function uc_multiPickerController($scope, $rootScope, $window, $location, $q, uCommerceContentService, $http, $timeout) {

    $scope.treeHeaderText = 'Available items';
    $scope.selectedItemsHeaderText = 'Selected items';
    $scope.selectedNodes = [];
    $scope.getPreSelectedValues = function () {
        return $scope.preSelectedValues;
    };

    $scope.getPreSelectedValuesString = function () {
        var preselectedValues = '';
        var first = true;

        for (n in $scope.preSelectedValues) {
            if (first) {
                first = false;
                preselectedValues += $scope.preSelectedValues[n];
            } else {
                preselectedValues += ',';
                preselectedValues += $scope.preSelectedValues[n];
            }
        }

        return preselectedValues;
    };

    $scope.$on('nodeSelected', function (event, data) {
        if ($scope.multiSelect == "false" && data.nodeType.toLowerCase().split(',').indexOf($scope.pickertype.toLowerCase()) != -1) {
            $scope.preSelectedValues = data.id;
        }
    });


    $scope.treeDisplayed = true;
    $scope.$on('startSearch',
        function (event) {
            $scope.treeDisplayed = false;
        });

    $scope.$on('stopSearch',
        function (event) {
            $scope.treeDisplayed = true;
        });

    $scope.updatePreselectedValues = function () {
        var preselectedValues = '';
        var first = true;

        for (n in $scope.selectedNodes) {
            if (first) {
                first = false;
                preselectedValues += $scope.selectedNodes[n].id;
            } else {
                preselectedValues += ',';
                preselectedValues += $scope.selectedNodes[n].id;
            }
        }
        $scope.preSelectedValues = preselectedValues;
    }

    $scope.$watch('selectedNodes', function () {
        if ($scope.selectedNodes.size > 0) {
            $scope.updatePreselectedValues();
        }
    });

    var intersectNodeTypes = function (firstNodeTypeString, secondNodeTypeString) {
        var firstNodeTypeArray = firstNodeTypeString.split(",");
        var secondNodeTypeArray = secondNodeTypeString.split(",");

        for (i in firstNodeTypeArray) {
            for (m in secondNodeTypeArray) {
                if (secondNodeTypeArray[m].toLowerCase() == firstNodeTypeArray[i].toLowerCase()) {
                    return true;
                }
            }
        }
    };

    $scope.$on('toggleSelectedNode', function (event, node) {

        for (n in $scope.selectedNodes) {
            var selectedNode = $scope.selectedNodes[n];
            if (selectedNode.id == node.id && intersectNodeTypes(selectedNode.nodeType, node.nodeType)) {
                $scope.selectedNodes.splice(n, 1);
                $scope.updatePreselectedValues();
                $scope.$broadcast('preSelectedValuesChanged', $scope.selectedNodes, $scope.hasCheckboxFor);
                $scope.$emit('preSelectedValuesChanged', $scope.selectedNodes, $scope.hasCheckboxFor);

                return;
            }
        }

        $scope.selectedNodes.push({
            id: node.id,
            name: node.name,
            nodeType: node.nodeType,
            icon: node.icon
        });

        $scope.$broadcast('preSelectedValuesChanged', $scope.selectedNodes, $scope.hasCheckboxFor);
        $scope.$emit('preSelectedValuesChanged', $scope.selectedNodes, $scope.hasCheckboxFor);
        $scope.updatePreselectedValues();
    });

    $scope.$on('setPreselectedValuesFromList',
        function (event, data, checkboxFor) {
            if ($scope.hasCheckboxFor === checkboxFor) {
                uCommerceContentService.getNodes($scope.hasCheckboxFor, data).then(function (response) {
                    var data = response.data;
                    data.forEach(function (element, index) {
                        $scope.selectedNodes.push({
                            id: element.id,
                            name: element.name,
                            nodeType: element.nodeType,
                            icon: element.icon
                        });
                    });

                    $scope.updatePreselectedValues();
                });
            }
        });

    $scope.loadPreselectedNodes = function () {

        var first = true;
        var preSelectedValues = $scope.getPreSelectedValues();
        var preSelectedValuesString = '';
        preSelectedValues.forEach(function (element, index) {
            if (first) {
                first = false;
                preSelectedValuesString += element;
            } else {
                preSelectedValuesString += ',';
                preSelectedValuesString += element;
            }
        });

        uCommerceContentService.getNodes($scope.hasCheckboxFor, preSelectedValuesString).then(function (response) {
            var data = response.data;
            data.forEach(function (element, index) {
                $scope.selectedNodes.push({
                    id: element.id,
                    name: element.name,
                    nodeType: element.nodeType,
                    icon: element.icon
                });
            });

            $scope.updatePreselectedValues();
        });
    }

    $scope.getNodeIconStyle = function (icon) {
        if ($scope.iconFolderOverwrite) {
            return $scope.iconFolderOverwrite + icon;
        }
        if (icon) {
            if (UCommerceClientMgr.Shell == 'Sitefinity') {
                if ($scope.iconFolder == 'uCommerce') {
                    var object = {
                        'background-image': 'url("' + UCommerceClientMgr.BaseUCommerceUrl + 'shell/content/images/ui/' + icon + '")'
                    };
                    return object;
                } else {
                    return {
                        'background-image': 'url("' + icon + '")'
                    };
                }
            }
        }
    }
}