(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory() :
        typeof define === 'function' && define.amd ? define(factory) :
            (global.VueFolderTree = factory());
}(this, (function () {
    var index = {
        name: 'foldertree',
        props: {
            folderlist: {
                required: true
            },
            cssclass: { required: true}
        },
        mounted() {

        },
        methods: {
            SetActiveFolder: function (folder, setFolderActive) {
                this.$emit('setactivefolder', folder);

                if (setFolderActive != null && setFolderActive == true) {
                    if (folder.isActive != undefined && folder.isActive != null) {
                        folder.isActive = !folder.isActive;
                    }
                    else {
                        folder.isActive = true;
                    }
                }
            }
        },
        template: `
    <ul :class="cssclass" v-if="folderlist != null && folderlist.length > 0">
        <li v-for="folder in folderlist">
            <span v-on:click="SetActiveFolder(folder, true)" v-if="folder.folderList != null && folder.folderList.length > 0" :class="['ds-caret', {'ds-caret-down': folder.isActive == true}]" style="cursor:pointer">{{folder.description}}</span>
            <span v-on:click="SetActiveFolder(folder)" v-if="folder.folderList == null || folder.folderList.length == 0" style="cursor:pointer">{{folder.description}}</span>
             <foldertree :cssclass="[{'ds-nested': folder.isActive == false}, {'ds-active':folder.isActive == true }]" :folderlist="folder.folderList" v-on:setactivefolder="SetActiveFolder"></foldertree>
        </li>
</ul>
`
    };
    return index;
})));