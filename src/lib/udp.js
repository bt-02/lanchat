const dgram = require("dgram")
const listen = dgram.createSocket("udp4")
const broadcast = dgram.createSocket("udp4")
const out = require("./out")

var list = []

module.exports = {

	//listen
	listen: function () {
		listen.on("error", function (err) {
			if (err.code === "EADDRINUSE") {
				out.warning("Port 2138 is busy. Lanchat can't listen for hosts.")
			}
		})
		listen.bind(2138)
		listen.on("message", function (msg, rinfo) {
			if (list.indexOf(rinfo.address) === -1) {
				out.status("Host detected in LAN: " + rinfo.address)
				list.push(rinfo.address)
			}
		})
	},

	//stop listen
	close: function () {
		listen.close()
	},

	//broadcast
	broadcast: function () {
		broadcast.on("listening", function () {
			broadcast.setBroadcast(true)
		})

		const message = "test"

		setInterval(function () {
			broadcast.send(message, 0, message.length, 2138)
		}, 500)
	},

	//list
	list: function () {
		for (var i = 0; i < list.length; i++) {
			out.status("[" + (i + 1) + "] " + list[i])
		}
	}
}
