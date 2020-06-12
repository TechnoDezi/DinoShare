(function (global, factory) {
    typeof exports === 'object' && typeof module !== 'undefined' ? module.exports = factory() :
        typeof define === 'function' && define.amd ? define(factory) :
            (global.VueCoolTable = factory());
}(this, (function () {
    var index = {
        name: 'cool-table',
        props: ['headers', 'items', 'tableClass', 'pagination'],
        data: function () {
            return {
                pageSizeList: [10, 20, 50, 100, 500],
                setPageSize: 10,
                pager: {}
            };
        },
        watch: {
            items: function (newItems) {
                var self = this;
                self.$nextTick(function () {
                    self.setPage(self.pager.currentPage, false);
                });
            },
            pagination: function () {
                var self = this;
                if (self.pagination.skip === 0 && self.$data.pager.currentPage > 1) {
                    this.setPage(1, false);
                }
            }
        },
        created: function() {
            this.$data.setPageSize = this.pagination.top;

            this.setPage(1, false);
        },
        methods: {
            onPageSizeChange: function(event) {
                this.pagination.skip = 0;
                this.pagination.top = this.$data.setPageSize;

                this.setPage(1, true);
            },
            onColClick: function(event, header) {
                this.pagination.sortBy = header.code;
                this.pagination.descending = !this.pagination.descending;

                this.$emit('pagechange', this.pagination);
            },
            setPage: function (page, emit) {
                if (emit === null || emit === undefined) {
                    emit = true;
                }
                if (page < 1 || page > this.pagination.totalRecords) {
                    return;
                }

                // get pager object from service
                this.$data.pager = this.getPager(page);

                this.pagination.skip = this.$data.pager.startIndex;

                // get current page of items
                if (emit) {
                    this.$emit('pagechange', this.pagination);
                }
            },
            getPager: function (currentPage) {
                if (currentPage === null || currentPage === undefined) {
                    currentPage = 1;
                }
                // calculate total pages
                var totalPages = Math.ceil(this.pagination.totalRecords / this.$data.setPageSize);

                var startPage, endPage;

                if (totalPages <= 5) {
                    startPage = 1;
                    endPage = totalPages;
                } else {
                    if (currentPage <= 3) {
                        startPage = 1;
                        endPage = 5;
                    } else if (currentPage + 1 >= totalPages) {
                        startPage = totalPages - 4;
                        endPage = totalPages;
                    } else {
                        startPage = currentPage - 2;
                        endPage = currentPage + 2;
                    }
                }

                // calculate start and end item indexes
                var startIndex = (currentPage - 1) * this.$data.setPageSize;
                var endIndex = Math.min(startIndex + this.$data.setPageSize - 1, this.pagination.totalRecords - 1);

                // create an array of pages to ng-repeat in the pager control
                var pages = _.range(startPage, endPage + 1);

                // return object with all pager properties required by the view
                return {
                    totalItems: this.pagination.totalRecords,
                    currentPage: currentPage,
                    pageSize: this.$data.setPageSize,
                    totalPages: totalPages,
                    startPage: startPage,
                    endPage: endPage,
                    startIndex: startIndex,
                    endIndex: endIndex,
                    pages: pages
                };
            }
        },
        template: `<div class="table-responsive-lg"><table v-bind:class="tableClass" cellspacing="0" width="100%">
            <thead>
                <tr>
                    <template v-for="header in headers">
                        <th v-if="header.disableSorting == false" v-on:click="onColClick($event, header)" style="cursor:pointer">
                            <i v-if="pagination.sortBy != null && pagination.sortBy == header.code && pagination.descending == true" class="fa fa-sort-up"></i> 
                            <i v-if="pagination.sortBy != null && pagination.sortBy == header.code && pagination.descending == false" class="fa fa-sort-down"></i> 
                            <span v-html="header.text"></span>
                        </th>
                        <th v-if="header.disableSorting == true" style="cursor:default" v-html="header.text"></th>
                    </template>
                </tr>
            </thead>
            <tbody>
                <slot name="body" v-bind:rowItems="items"></slot>
            </tbody>
            <tfoot>
                <slot name="footer"></slot>
            </tfoot>
        </table>
        <div class="row">
            <div class="col-xs-6">
                <select v-model="setPageSize" v-on:change="onPageSizeChange($event)" style="width: 50px" class="no-chosen">
                    <option v-for="option in pageSizeList" :value="option">{{option}}</option>
                </select> per page | Showing results {{pagination.skip + 1}} to {{pagination.skip + setPageSize}} of {{pagination.totalRecords}}
            </div>
            <div class="col-xs-6 text-right">
                <ul v-if="pager.pages && pager.pages.length" class="pagination">
                    <li v-bind:class="[{'disabled': pager.currentPage === 1}]">
                        <a v-on:click="setPage(1)"><i class="fa fa-chevron-left"></i><i class="fa fa-chevron-left"></i></a>
                    </li>
                    <li v-bind:class="[{'disabled': pager.currentPage === 1}]">
                        <a v-on:click="setPage(pager.currentPage - 1)"><i class="fa fa-chevron-left"></i></a>
                    </li>
                    <li v-for="page in pager.pages" v-bind:class="[{'active': pager.currentPage === page}]">
                        <a v-on:click="setPage(page)">{{page}}</a>
                    </li>
                    <li v-bind:class="[{'disabled': pager.currentPage === pager.totalPages}]">
                        <a v-on:click="setPage(pager.currentPage + 1)"><i class="fa fa-chevron-right"></i></a>
                    </li>
                    <li v-bind:class="[{'disabled': pager.currentPage === pager.totalPages}]">
                        <a v-on:click="setPage(pager.totalPages)"><i class="fa fa-chevron-right"></i><i class="fa fa-chevron-right"></i></a>
                    </li>
                </ul>
            </div>
        </div>
</div>`
    };
    return index;
})));