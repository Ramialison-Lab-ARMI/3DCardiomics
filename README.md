# 3D-Cardiomics
## A visual system for the exploration of cardiac transcriptome data


### Overview
3D-Cardiomics provides a framework to map multi dimensional data (such as intensity and location of gene expression) on to 3D models in a heatmap-like manner. In our use case the intensity is the level of gene expression of those expressed in the adult heart, and the location is this expression level in 18 discrete pieces of the heart as measured by RNA-seq.

### Important Files
The expression values are stored in
```
Assets/Resources/fake_mouse_expression_data.txt
```
The format of this file is tab-seperated beginning with the item name, and its intensity in the (18) locations. A sample file is provided with a few randomised values to demonstrate the functionality, pending final publication of the results.

### Dependencies
- Unity (we are using 2018.4.23.f1)
- Any modern web browser which is WebGL capable

### Instructions
Simply run unity and open the root folder of the project. If the heart scene is not visible, open it in unity by browsing to and double clicking the following file:
```
Assets/3DCardiomics_main.unity
```



