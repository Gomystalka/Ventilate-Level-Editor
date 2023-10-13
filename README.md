# Ventilate Level Editor
 This is a Level Editor for my game Ventilate.
 <b><h1>DISCLAIMER: THE VERSION ON THE REPOSITORY MAY NOT BE ENTIRELY UP-TO-DATE AS THIS PROJECT WAS MIGRATED TO THE VENTILATE GAME PROJECT ITSELF.</h1></b>

<h1>Functions</h1>
<ol>
 <li>Asset Placement Window</li>
 <li>Quad Mesh Creator</li>
 <li>Combat Area Creator</li>
</ol>

<h1>Asset Placement Window</h1>
<p>This window can be used to simplify the placement of props when building levels.</p>
<h2>Usage</h2>
<ol>
 <li>Create a new <b>Asset Data</b> object via the Unity Create menu.</li>
 <li>Open the <b>Asset Importer</b> window.</li>
 <li>Drag in any assets into the <b>Asset Importer</b> window and assign categories accordingly.</li>
 <li>Import the assets by clicking <b>'Import'</b></li>
 <li>Reopen the Asset Placement window, drag in the <b>Asset Data</b> you just created and begin the placement of props.</li>
</ol>
![image](https://github.com/Gomystalka/Ventilate-Level-Editor/assets/48233064/a415c16d-b412-416e-905d-45480586cfff)

<h1>Quad Mesh Creator</h1>
<p>This window can be used to simplify the creation process of the path meshes used for the calculation of collisions and navigation meshes.</p>
<h2>Usage</h2>
<ol>
 <li>Create the source quad by clicking the <b>'Create Source Quad' button inside of the window. A single quad will now appear at the position of the path creator object. <strong>This object is hidden by default, to reveal it, please tick <i>Show Path Creator Object in Hierarchy</i> within the debug menu.</strong></b></li>
 <li>Once the source quad is created, click on any of the visible position markers to create a quad in that direction.</li>
 <li>Once you are happy with the result, click on 'Generate Mesh' and follow the on-screen instructions to change the mesh you created into a Unity-usable mesh.</li>
</ol>
<p>These quads more on a by-vertex basis, meaning that combination of quads and movement is done via the vertices. To combine two vertices, select the two vertices and click 'Combine' in the menu that appears. </p>
<p>To disconnect two or more vertices, simply select a vertex and click on 'Disconnect' in the menu that appears.</p>
<p>Quads can also be deleted by selecting the quad's source vertex and click on 'Delete Quad'. Be careful however, as this feature is unstable.</p>
<p>This window also offers saving, autosaving and loading of meshes.</p>
<strong><p>VERTICES SHOULD NOT BE DELETED MANUALLY! PLEASE USE THE VERTEX MANIPULATION WINDOW INSTEAD!</p></strong>

<h1>Combat Area Creator</h1>
<p>This window can be used to simplify the creation of combat zones across levels.</p>
<h2>Usage</h2>
<ol>
 <li>Click on 'Create Combat Area'. This will create a combat area gameobject within the scene. Each combat area object has its own properties when selected. The selection determines what appears within the window.</li>
 <li>Once created, to create the boundaries of the combat area, simply click on the directional arrow which will create another arrow in the direction of the first one and a new vertex which can be manipulated.</li>
 <li>Once you are happy with the shape of the combat area, click 'Finalise' to finalise the design of the combat area.</li>
 <li>After finalisation, you can add enemies and triggers accordingly.</li>
</ol>
<p>Unlike the Quad Mesh Creator, the vertices can be freely deleted without any issues.</p>
