# 3D Cardiomics: A system for the exploration of cardiac transcriptome data

### Overview
3D Cardiomics provides a framework to map multi dimensional data (intensity/location) on to 3D models like a heatmap. In our use case the intensity is the level of gene expression of those expressed in the adult heart and the location is this expression level in 18 discrete pieces of the heart as measured by RNA-seq.

### Important Files
The expression levels are stored in
```
Assets/Resources/fernP4.txt
```
The format of this file is tab-seperated beginning with the item name, and its intensity in the (18) locations. A sample file is provided with a few randomised values to demonstrate the functionality pending final publication of the results.

### Dependencies
- Unity (we are using 5.5.0f3)
- Any modern web browser which is WebGL capable

### Instructions
Simply run unity and open the root folder of the project. If the heart scene is not visible, open it in unity by browsing to:
```
Assets/3DCardiomics_main.unity
```
and double clicking the file



