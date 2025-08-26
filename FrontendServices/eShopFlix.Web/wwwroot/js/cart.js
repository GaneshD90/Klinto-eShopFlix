function addToCart(ItemId, Name, UnitPrice, Quantity) {
    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        url: '/Cart/AddToCart/' + ItemId + "/" + UnitPrice + "/" + Quantity,
        success: function (response) {
            if (response != undefined && response.status === 'success') {
                var counter = response.count;
                $("#cartCounter").text(counter);
            }
        },
        error: function (result) {
        }
    });
}
function deleteItem(id, cartId) {
    if (id > 0) {
        $.ajax({
            type: "GET",
            url: '/Cart/DeleteItem/' + id + "/" + cartId,
            success: function (data) {
                if (data > 0) {
                    location.reload();
                }
            }
        });
    }
}
function updateQuantity(id, currentQuantity, quantity, cartId) {
    if ((currentQuantity >= 1 && quantity == 1) || (currentQuantity > 1 && quantity == -1)) {
        $.ajax({
            url: '/Cart/UpdateQuantity/' + id + "/" + quantity + "/" + cartId,
            type: 'GET',
            success: function (response) {
                if (response > 0) {
                    location.reload();
                }
            }
        });
    }
}

$(document).ready(function () {
    $.ajax({
        type: "GET",
        contentType: "application/json; charset=utf-8",
        url: '/Cart/GetCartCount',
        success: function (data) {
            $("#cartCounter").text(data);
        },
        error: function (result) {
        },
    });
});